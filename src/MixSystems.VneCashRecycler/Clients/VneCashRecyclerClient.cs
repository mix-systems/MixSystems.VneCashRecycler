using System.Text.Json;
using Microsoft.Extensions.Logging;
using MixSystems.VneCashRecycler.Configuration;
using MixSystems.VneCashRecycler.Exceptions;
using MixSystems.VneCashRecycler.Http;
using MixSystems.VneCashRecycler.Models;
using MixSystems.VneCashRecycler.Protocol.Requests;
using MixSystems.VneCashRecycler.Protocol.Responses;
using MixSystems.VneCashRecycler.ValueObjects;

namespace MixSystems.VneCashRecycler.Clients;

internal sealed class VneCashRecyclerClient(
    HttpClient httpClient,
    VneCashRecyclerClientOptions options,
    VneHttpRequestExecutor executor,
    ILogger<VneCashRecyclerClient> logger)
    : IVneCashRecyclerClient
{
    private readonly Uri _endpoint = options.Endpoint;
    private readonly OperatorName? _defaultOperator = options.DefaultOperatorName;
    private readonly ILogger<VneCashRecyclerClient> _logger = logger;

    public async Task<PaymentRequestResult> SendPaymentAsync(
        MoneyCents amount,
        OperatorName? operatorName = null,
        CancellationToken cancellationToken = default,
        PaymentRequestOptions? options = null,
        OperatorIdentifier? operatorIdentifier = null)
    {
        int? refundable = MapNullableBoolean(options?.IsRefundable);
        int? creditCard = MapNullableBoolean(options?.UseCreditCard);
        string? authKey = string.IsNullOrWhiteSpace(options?.AuthenticationKey)
            ? null
            : options!.AuthenticationKey!.Trim();

        PaymentRequestMessage payload = new(
            VneRequestType.PaymentRequest,
            amount.Value,
            ResolveOperator(operatorName),
            operatorIdentifier?.Value,
            refundable,
            creditCard,
            authKey);

        AcknowledgementResponse response = await executor.PostAsync<PaymentRequestMessage, AcknowledgementResponse>(httpClient, _endpoint, payload, cancellationToken).ConfigureAwait(false);
        EnsureAck(response);

        if (response.PaymentId is null)
        {
            throw new VneApiException(response.RequestType, response.ErrorCode, "Missing payment identifier in ACK response.");
        }

        return new PaymentRequestResult(new PaymentId(response.PaymentId), new MoneyCents(response.Amount ?? amount.Value));
    }

    public async Task<PaymentProgress> GetPaymentStatusAsync(PaymentId paymentId, OperatorName? operatorName = null, CancellationToken cancellationToken = default)
    {
        PaymentStatusRequestMessage payload = new(VneRequestType.PaymentStatusPolling, paymentId.Value, ResolveOperator(operatorName));

        PaymentStatusResponse response = await executor.PostAsync<PaymentStatusRequestMessage, PaymentStatusResponse>(httpClient, _endpoint, payload, cancellationToken).ConfigureAwait(false);
        EnsureAck(response.RequestType, response.RequestStatus, response.ErrorCode, response.Operation, response.OperationUser);

        PaymentLifecycleStatus lifecycle = response.PaymentStatus == 1 ? PaymentLifecycleStatus.Completed : PaymentLifecycleStatus.Pending;
        PaymentDetailStatus detail = ParsePaymentDetailStatus(response.Details.Status);
        PaymentTenderType tender = response.Details.Inserted == -2 ? PaymentTenderType.Card : PaymentTenderType.Cash;
        MoneyCents inserted = tender == PaymentTenderType.Card
            ? new MoneyCents(0)
            : new MoneyCents(Math.Max(0, response.Details.Inserted));
        MoneyCents rest = new(Math.Max(0, response.Details.Rest));

        return new PaymentProgress(
            paymentId,
            lifecycle,
            detail,
            new MoneyCents(response.Details.Amount),
            inserted,
            rest,
            response.Details.NotReturned is 1,
            response.Details.CancelledBy,
            tender);
    }

    public async Task<PaymentCancellationResult> CancelPaymentAsync(PaymentId paymentId, PaymentCancellationMode cancellationMode, OperatorName? operatorName = null, CancellationToken cancellationToken = default)
    {
        DeletePaymentRequestMessage payload = new(VneRequestType.DeletePendingPayment, paymentId.Value, (int)cancellationMode, ResolveOperator(operatorName));
        PaymentCancellationResponse response = await executor.PostAsync<DeletePaymentRequestMessage, PaymentCancellationResponse>(httpClient, _endpoint, payload, cancellationToken).ConfigureAwait(false);
        EnsureAck(response.RequestType, response.RequestStatus, response.ErrorCode, response.Operation, response.OperationUser);

        if (!response.PaymentStatus.HasValue)
        {
            throw new VneApiException(response.RequestType, response.ErrorCode, "Missing payment_status in cancellation response.");
        }

        PaymentCancellationStatus status = MapCancellationStatus(response.PaymentStatus.Value);
        return new PaymentCancellationResult(paymentId, status);
    }

    public async Task<PendingPaymentList> ListPendingPaymentsAsync(OperatorName? operatorName = null, CancellationToken cancellationToken = default)
    {
        PendingPaymentsRequestMessage payload = new(VneRequestType.PendingPaymentsList, ResolveOperator(operatorName));
        PendingPaymentsResponse response = await executor.PostAsync<PendingPaymentsRequestMessage, PendingPaymentsResponse>(httpClient, _endpoint, payload, cancellationToken).ConfigureAwait(false);
        EnsureAck(response.RequestType, response.RequestStatus, response.ErrorCode, response.Operation, response.OperationUser);

        List<PaymentId> ids = [];
        if (response.PendingList is not null)
        {
            foreach ((_, string paymentId) in response.PendingList.OrderBy(kvp => ExtractListIndex(kvp.Key)))
            {
                ids.Add(new PaymentId(paymentId));
            }
        }

        return new PendingPaymentList(ids);
    }

    public async Task<MachineStatus> GetMachineStatusAsync(OperatorName? operatorName = null, CancellationToken cancellationToken = default)
    {
        MachineStatusRequestMessage payload = new(VneRequestType.MachineStatus, ResolveOperator(operatorName));
        MachineStatusResponse response = await executor.PostAsync<MachineStatusRequestMessage, MachineStatusResponse>(httpClient, _endpoint, payload, cancellationToken).ConfigureAwait(false);
        EnsureAck(response.RequestType, response.RequestStatus, response.ErrorCode, response.Operation, response.OperationUser);
        return MapMachineStatus(response);
    }

    public async Task<WithdrawalRequestResult> StartWithdrawalAsync(
        MoneyCents amount,
        WithdrawalDenomination? payoutMode = null,
        OperatorName? operatorName = null,
        CancellationToken cancellationToken = default,
        OperatorIdentifier? operatorIdentifier = null)
    {
        WithdrawalRequestMessage payload = new(
            VneRequestType.OperatorWithdrawalRequest,
            amount.Value,
            ResolveOperator(operatorName),
            payoutMode?.Value,
            operatorIdentifier?.Value);

        AcknowledgementResponse response = await executor.PostAsync<WithdrawalRequestMessage, AcknowledgementResponse>(httpClient, _endpoint, payload, cancellationToken).ConfigureAwait(false);
        EnsureAck(response);

        if (response.PaymentId is null)
        {
            throw new VneApiException(response.RequestType, response.ErrorCode, "Missing withdrawal identifier.");
        }

        return new WithdrawalRequestResult(new WithdrawalId(response.PaymentId), new MoneyCents(response.Amount ?? amount.Value));
    }

    public async Task<WithdrawalProgress> GetWithdrawalStatusAsync(
        WithdrawalId withdrawalId,
        OperatorName? operatorName = null,
        CancellationToken cancellationToken = default)
    {
        WithdrawalStatusRequestMessage payload = new(VneRequestType.OperatorWithdrawalPolling, withdrawalId.Value, ResolveOperator(operatorName));
        WithdrawalStatusResponse response = await executor.PostAsync<WithdrawalStatusRequestMessage, WithdrawalStatusResponse>(httpClient, _endpoint, payload, cancellationToken).ConfigureAwait(false);
        EnsureAck(response.RequestType, response.RequestStatus, response.ErrorCode, response.Operation, response.OperationUser);

        WithdrawalStatus status = response.Status == 1 ? WithdrawalStatus.Completed : WithdrawalStatus.NotCompleted;
        MoneyCents requested = response.Details is not null
            ? new MoneyCents(response.Details.Amount)
            : new MoneyCents(0);
        MoneyCents withdrawn = response.Details is not null
            ? new MoneyCents(response.Details.Withdrawn)
            : new MoneyCents(0);

        return new WithdrawalProgress(new WithdrawalId(response.WithdrawalId), status, requested, withdrawn);
    }

    public async Task EndWithdrawalAsync(OperatorName? operatorName = null, CancellationToken cancellationToken = default)
    {
        EndWithdrawalRequestMessage payload = new(VneRequestType.EndOperatorWithdrawal, ResolveOperator(operatorName));
        AcknowledgementResponse response = await executor.PostAsync<EndWithdrawalRequestMessage, AcknowledgementResponse>(httpClient, _endpoint, payload, cancellationToken).ConfigureAwait(false);
        EnsureAck(response);
    }

    public async Task StartRefillAsync(bool acceptAll = false, OperatorName? operatorName = null, CancellationToken cancellationToken = default)
    {
        StartRefillRequestMessage payload = new(
            VneRequestType.StartRefill,
            ResolveOperator(operatorName),
            acceptAll ? 1 : 0);

        AcknowledgementResponse response = await executor.PostAsync<StartRefillRequestMessage, AcknowledgementResponse>(httpClient, _endpoint, payload, cancellationToken).ConfigureAwait(false);
        EnsureAck(response);
    }

    public async Task EndRefillAsync(OperatorName? operatorName = null, CancellationToken cancellationToken = default)
    {
        EndRefillRequestMessage payload = new(VneRequestType.EndRefill, ResolveOperator(operatorName));
        AcknowledgementResponse response = await executor.PostAsync<EndRefillRequestMessage, AcknowledgementResponse>(httpClient, _endpoint, payload, cancellationToken).ConfigureAwait(false);
        EnsureAck(response);
    }

    public async Task ManualRefillAsync(ManualRefillRequest request, OperatorName? operatorName = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        ManualRefillRequestMessage payload = new(
            VneRequestType.ManualRefill,
            ResolveOperator(operatorName),
            request.Values);

        AcknowledgementResponse response = await executor.PostAsync<ManualRefillRequestMessage, AcknowledgementResponse>(httpClient, _endpoint, payload, cancellationToken).ConfigureAwait(false);
        EnsureAck(response);
    }

    public async Task<RefillPollingResult> PollRefillAsync(CancellationToken cancellationToken = default)
    {
        RefillStatusRequestMessage payload = new(VneRequestType.RefillPolling);
        RefillStatusResponse response = await executor.PostAsync<RefillStatusRequestMessage, RefillStatusResponse>(httpClient, _endpoint, payload, cancellationToken).ConfigureAwait(false);
        EnsureAck(response.RequestType, response.RequestStatus, response.ErrorCode, response.Operation, response.OperationUser);

        bool refillOn = response.RefillOn == 1;
        MoneyCents amount = response.AmountRefill.HasValue ? new MoneyCents(response.AmountRefill.Value) : new MoneyCents(0);
        return new RefillPollingResult(refillOn, amount);
    }

    public async Task LoginOperatorAsync(OperatorName? operatorName = null, CancellationToken cancellationToken = default)
    {
        OperatorLoginRequestMessage payload = new(VneRequestType.OperatorLogin, ResolveOperator(operatorName));
        AcknowledgementResponse response = await executor.PostAsync<OperatorLoginRequestMessage, AcknowledgementResponse>(httpClient, _endpoint, payload, cancellationToken).ConfigureAwait(false);
        EnsureAck(response);
    }

    public async Task LogoutOperatorAsync(OperatorName? operatorName = null, CancellationToken cancellationToken = default)
    {
        OperatorLogoutRequestMessage payload = new(VneRequestType.OperatorLogout, ResolveOperator(operatorName));
        AcknowledgementResponse response = await executor.PostAsync<OperatorLogoutRequestMessage, AcknowledgementResponse>(httpClient, _endpoint, payload, cancellationToken).ConfigureAwait(false);
        EnsureAck(response);
    }

    public async Task StartHopperEmptyingAsync(bool emptyToPettyCash = false, OperatorName? operatorName = null, CancellationToken cancellationToken = default)
    {
        HopperEmptyingRequestMessage payload = new(VneRequestType.HopperEmptying, ResolveOperator(operatorName), emptyToPettyCash ? 1 : 0);
        AcknowledgementResponse response = await executor.PostAsync<HopperEmptyingRequestMessage, AcknowledgementResponse>(httpClient, _endpoint, payload, cancellationToken).ConfigureAwait(false);
        EnsureAck(response);
    }

    public async Task StartRecyclerEmptyingAsync(bool emptyToPettyCash = false, OperatorName? operatorName = null, CancellationToken cancellationToken = default)
    {
        RecyclerEmptyingRequestMessage payload = new(VneRequestType.RecyclerEmptying, ResolveOperator(operatorName), emptyToPettyCash ? 1 : 0);
        AcknowledgementResponse response = await executor.PostAsync<RecyclerEmptyingRequestMessage, AcknowledgementResponse>(httpClient, _endpoint, payload, cancellationToken).ConfigureAwait(false);
        EnsureAck(response);
    }

    public async Task ResetStackerAsync(StackerPeripheral peripheral, OperatorName? operatorName = null, CancellationToken cancellationToken = default)
    {
        StackerCancellationRequestMessage payload = new(VneRequestType.StackerCancellation, (int)peripheral, ResolveOperator(operatorName));
        AcknowledgementResponse response = await executor.PostAsync<StackerCancellationRequestMessage, AcknowledgementResponse>(httpClient, _endpoint, payload, cancellationToken).ConfigureAwait(false);
        EnsureAck(response);
    }

    public async Task<EmptyingStatusResult> PollEmptyingAsync(CancellationToken cancellationToken = default)
    {
        EmptyingStatusRequestMessage payload = new(VneRequestType.EmptyingPolling);
        EmptyingStatusResponse response = await executor.PostAsync<EmptyingStatusRequestMessage, EmptyingStatusResponse>(httpClient, _endpoint, payload, cancellationToken).ConfigureAwait(false);
        EnsureAck(response.RequestType, response.RequestStatus, response.ErrorCode, response.Operation, response.OperationUser);

        bool inProgress = response.EmptyStatus is 1;
        return new EmptyingStatusResult(inProgress);
    }

    public async Task StartCoinDispenserEmptyingAsync(CoinDispenserSelection selection, OperatorName? operatorName = null, CancellationToken cancellationToken = default)
    {
        CoinDispenserEmptyingRequestMessage payload = new(VneRequestType.CoinDispenserEmptying, ResolveOperator(operatorName), (int)selection);
        AcknowledgementResponse response = await executor.PostAsync<CoinDispenserEmptyingRequestMessage, AcknowledgementResponse>(httpClient, _endpoint, payload, cancellationToken).ConfigureAwait(false);
        EnsureAck(response);
    }

    public async Task OpenDoorAsync(TimeSpan? waitTimeout = null, int? port = null, OperatorName? operatorName = null, CancellationToken cancellationToken = default)
    {
        DoorOpeningRequestMessage payload = new(
            VneRequestType.DoorOpening,
            FormatDoorTimeout(waitTimeout),
            ResolveOperator(operatorName),
            port);

        AcknowledgementResponse response = await executor.PostAsync<DoorOpeningRequestMessage, AcknowledgementResponse>(httpClient, _endpoint, payload, cancellationToken).ConfigureAwait(false);
        EnsureAck(response);
    }

    public async Task<CashClosingReport> GetCashClosingAsync(OperatorName? operatorName = null, CancellationToken cancellationToken = default)
    {
        CashClosingRequestMessage payload = new(VneRequestType.CashClosing, ResolveOperator(operatorName));
        CashClosingResponse response = await executor.PostAsync<CashClosingRequestMessage, CashClosingResponse>(httpClient, _endpoint, payload, cancellationToken).ConfigureAwait(false);
        EnsureAck(response.RequestType, response.RequestStatus, response.ErrorCode, response.Operation, response.OperationUser);

        DateTimeOffset date = FromUnixSeconds(response.Date);
        DateTimeOffset startDate = FromUnixSeconds(response.StartDate);

        (MoneyCents? pettyCash, bool restored) = ParsePettyCash(response.PettyCash);

        return new CashClosingReport(
            date,
            startDate,
            ToMoney(response.TotalIn),
            ToMoney(response.TotalOut),
            ToMoney(response.TotalPayments),
            ToMoney(response.TotalOperatorIn),
            ToMoney(response.TotalOperatorOut),
            ToMoney(response.TotalContent),
            pettyCash,
            restored,
            ToMoney(response.CashIncome));
    }

    public async Task StartRefundAsync(PaymentId paymentId, OperatorName? operatorName = null, CancellationToken cancellationToken = default)
    {
        RefundRequestMessage payload = new(VneRequestType.Refund, paymentId.Value, ResolveOperator(operatorName));
        AcknowledgementResponse response = await executor.PostAsync<RefundRequestMessage, AcknowledgementResponse>(httpClient, _endpoint, payload, cancellationToken).ConfigureAwait(false);
        EnsureAck(response);
    }

    public async Task<RefundProgress> PollRefundAsync(PaymentId paymentId, CancellationToken cancellationToken = default)
    {
        RefundStatusRequestMessage payload = new(VneRequestType.RefundPolling, paymentId.Value);
        RefundStatusResponse response = await executor.PostAsync<RefundStatusRequestMessage, RefundStatusResponse>(httpClient, _endpoint, payload, cancellationToken).ConfigureAwait(false);
        EnsureAck(response.RequestType, response.RequestStatus, response.ErrorCode, response.Operation, response.OperationUser);

        RefundStatus status = response.RefundStatus?.Equals("completed", StringComparison.OrdinalIgnoreCase) == true
            ? RefundStatus.Completed
            : RefundStatus.Pending;

        MoneyCents refunded = ToMoney(response.Refunded);
        MoneyCents toRefund = ToMoney(response.ToRefund);

        return new RefundProgress(new PaymentId(response.PaymentId), status, refunded, toRefund);
    }

    public async Task<WithdrawalReport> GetWithdrawalsAsync(DateTimeOffset? startDate, DateTimeOffset? endDate, OperatorName? operatorName = null, CancellationToken cancellationToken = default)
    {
        (DateTimeOffset rangeStart, DateTimeOffset rangeEnd) = RequireTimelineRange(startDate, endDate, nameof(startDate), nameof(endDate));
        TimelineRequestMessage payload = BuildRangePayload(VneRequestType.WithdrawalsList, rangeStart, rangeEnd, operatorName ?? _defaultOperator);
        WithdrawalListResponse response = await executor.PostAsync<TimelineRequestMessage, WithdrawalListResponse>(httpClient, _endpoint, payload, cancellationToken).ConfigureAwait(false);
        EnsureAck(response.RequestType, response.RequestStatus, response.ErrorCode, response.Operation, response.OperationUser);

        IReadOnlyList<WithdrawalEntry> entries = response.Withdrawals is null
            ? []
            : response.Withdrawals.Select(w => new WithdrawalEntry(FromUnixSeconds(w.Date), new MoneyCents(w.Value), w.Operator)).ToArray();

        return new WithdrawalReport(entries);
    }

    public async Task<PaymentReport> GetPaymentsAsync(DateTimeOffset? startDate, DateTimeOffset? endDate, OperatorName? operatorName = null, CancellationToken cancellationToken = default)
    {
        (DateTimeOffset rangeStart, DateTimeOffset rangeEnd) = RequireTimelineRange(startDate, endDate, nameof(startDate), nameof(endDate));
        TimelineRequestMessage payload = BuildRangePayload(VneRequestType.PaymentsList, rangeStart, rangeEnd, operatorName ?? _defaultOperator);
        PaymentListResponse response = await executor.PostAsync<TimelineRequestMessage, PaymentListResponse>(httpClient, _endpoint, payload, cancellationToken).ConfigureAwait(false);
        EnsureAck(response.RequestType, response.RequestStatus, response.ErrorCode, response.Operation, response.OperationUser);

        IReadOnlyList<PaymentEntry> entries = response.Payments is null
            ? []
            : response.Payments.Select(p =>
            {
                PaymentTenderType tender = p.Inserted == -2 ? PaymentTenderType.Card : PaymentTenderType.Cash;
                MoneyCents inserted = tender == PaymentTenderType.Card ? new MoneyCents(0) : ToMoney(p.Inserted);

                return new PaymentEntry(
                    FromUnixSeconds(p.Date),
                    new PaymentId(p.PaymentId),
                    p.Status,
                    ToMoney(p.Amount),
                    inserted,
                    ToMoney(p.Rest),
                    ToMoney(p.Refund),
                    p.Operator,
                    p.CancelledBy,
                    tender);
            }).ToArray();

        return new PaymentReport(entries);
    }

    public async Task<OpeningReport> GetOpeningsAsync(DateTimeOffset? startDate, DateTimeOffset? endDate, OperatorName? operatorName = null, CancellationToken cancellationToken = default)
    {
        (DateTimeOffset rangeStart, DateTimeOffset rangeEnd) = RequireTimelineRange(startDate, endDate, nameof(startDate), nameof(endDate));
        TimelineRequestMessage payload = BuildRangePayload(VneRequestType.OpeningsList, rangeStart, rangeEnd, operatorName ?? _defaultOperator);
        OpeningListResponse response = await executor.PostAsync<TimelineRequestMessage, OpeningListResponse>(httpClient, _endpoint, payload, cancellationToken).ConfigureAwait(false);
        EnsureAck(response.RequestType, response.RequestStatus, response.ErrorCode, response.Operation, response.OperationUser);

        IReadOnlyList<OpeningEntry> entries = response.Openings is null
            ? []
            : response.Openings.Select(o => new OpeningEntry(FromUnixSeconds(o.Date), (OpeningEventType)o.Type, o.Operator)).ToArray();

        return new OpeningReport(entries);
    }

    public async Task<CashClosingHistoryReport> GetCashClosingHistoryAsync(DateTimeOffset? startDate, DateTimeOffset? endDate, OperatorName? operatorName = null, CancellationToken cancellationToken = default)
    {
        (DateTimeOffset rangeStart, DateTimeOffset rangeEnd) = RequireTimelineRange(startDate, endDate, nameof(startDate), nameof(endDate));
        TimelineRequestMessage payload = BuildRangePayload(VneRequestType.CashClosingList, rangeStart, rangeEnd, operatorName ?? _defaultOperator);
        CashClosingHistoryResponse response = await executor.PostAsync<TimelineRequestMessage, CashClosingHistoryResponse>(httpClient, _endpoint, payload, cancellationToken).ConfigureAwait(false);
        EnsureAck(response.RequestType, response.RequestStatus, response.ErrorCode, response.Operation, response.OperationUser);

        IReadOnlyList<CashClosingEntry> entries = response.Closings is null
            ? Array.Empty<CashClosingEntry>()
            : response.Closings.Select(c =>
            {
                (MoneyCents? petty, bool restored) = ParsePettyCash(c.PettyCash);
                return new CashClosingEntry(
                    c.Operator,
                    FromUnixSeconds(c.Date),
                    FromUnixSeconds(c.StartDate),
                    ToMoney(c.TotalIn),
                    ToMoney(c.TotalOut),
                    ToMoney(c.TotalPayments),
                    ToMoney(c.TotalOperatorIn),
                    ToMoney(c.TotalOperatorOut),
                    ToMoney(c.TotalContent),
                    petty,
                    restored,
                    ToMoney(c.CashIncome));
            }).ToArray();

        return new CashClosingHistoryReport(entries);
    }

    public async Task AccessConfigurationMenuAsync(int userLevel, OperatorName? operatorName = null, string? password = null, CancellationToken cancellationToken = default)
    {
        string? resolvedOperator = ResolveOperator(operatorName);
        string? sanitizedPassword = password?.Trim();
        EnsureMenuCredentials(userLevel, operatorName, resolvedOperator, sanitizedPassword, nameof(password));

        MenuAccessRequestMessage payload = new(VneRequestType.AccessConfigurationMenu, userLevel, resolvedOperator, sanitizedPassword);
        AcknowledgementResponse response = await executor.PostAsync<MenuAccessRequestMessage, AcknowledgementResponse>(httpClient, _endpoint, payload, cancellationToken).ConfigureAwait(false);
        EnsureAck(response);
    }

    public async Task RebootOrShutdownAsync(bool restart, OperatorName? operatorName = null, CancellationToken cancellationToken = default)
    {
        RebootRequestMessage payload = new(VneRequestType.RebootOrShutdown, ResolveOperator(operatorName), restart ? 1 : 0);
        AcknowledgementResponse response = await executor.PostAsync<RebootRequestMessage, AcknowledgementResponse>(httpClient, _endpoint, payload, cancellationToken).ConfigureAwait(false);
        EnsureAck(response);
    }

    public async Task<string?> GetSoftwareVersionAsync(OperatorName? operatorName = null, CancellationToken cancellationToken = default)
    {
        VersionRequestMessage payload = new(VneRequestType.MachineVersion, ResolveOperator(operatorName));
        VersionResponse response = await executor.PostAsync<VersionRequestMessage, VersionResponse>(httpClient, _endpoint, payload, cancellationToken).ConfigureAwait(false);
        EnsureAck(response.RequestType, response.RequestStatus, response.ErrorCode, response.Operation, response.OperationUser);
        return response.Version;
    }

    public async Task ConfigurePeripheralsAsync(PeripheralsConfiguration configuration, OperatorName? operatorName = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        PeripheralsConfigurationRequestMessage payload = new(VneRequestType.PeripheralsConfiguration, ResolveOperator(operatorName), configuration.Values);
        AcknowledgementResponse response = await executor.PostAsync<PeripheralsConfigurationRequestMessage, AcknowledgementResponse>(httpClient, _endpoint, payload, cancellationToken).ConfigureAwait(false);
        EnsureAck(response);
    }

    public async Task ConfigureSoftwareAsync(SoftwareConfiguration configuration, OperatorName? operatorName = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        SoftwareConfigurationRequestMessage payload = new(VneRequestType.SoftwareConfiguration, ResolveOperator(operatorName), configuration.Values);
        AcknowledgementResponse response = await executor.PostAsync<SoftwareConfigurationRequestMessage, AcknowledgementResponse>(httpClient, _endpoint, payload, cancellationToken).ConfigureAwait(false);
        EnsureAck(response);
    }

    public async Task<MachineStatus> SetPettyCashAsync(MoneyCents pettyCash, OperatorName? operatorName = null, CancellationToken cancellationToken = default)
    {
        PettyCashSettingRequestMessage payload = new(VneRequestType.PettyCashSetting, ResolveOperator(operatorName), pettyCash.Value);
        MachineStatusResponse response = await executor.PostAsync<PettyCashSettingRequestMessage, MachineStatusResponse>(httpClient, _endpoint, payload, cancellationToken).ConfigureAwait(false);
        EnsureAck(response.RequestType, response.RequestStatus, response.ErrorCode, response.Operation, response.OperationUser);
        return MapMachineStatus(response);
    }

    public async Task SignalEndPettyCashRestoreAsync(CancellationToken cancellationToken = default)
    {
        EndPettyCashRestoreRequestMessage payload = new(VneRequestType.EndPettyCashRestore);
        AcknowledgementResponse response = await executor.PostAsync<EndPettyCashRestoreRequestMessage, AcknowledgementResponse>(httpClient, _endpoint, payload, cancellationToken).ConfigureAwait(false);
        EnsureAck(response);
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        httpClient.Dispose();
    }

    private string? ResolveOperator(OperatorName? operatorName)
        => operatorName?.Value ?? _defaultOperator?.Value;

    private static MachineStatus MapMachineStatus(MachineStatusResponse response)
    {
        RecyclerStatus? recycler = response.Recycler is null ? null : MapRecycler(response.Recycler);
        HopperStatus? hopper = response.Hopper is null ? null : MapHopper(response.Hopper);
        DispenserStatus? dispenser = response.Dispenser is null ? null : MapDispenser(response.Dispenser);
        CoinDispenserStatus? coinDispenser = response.CoinDispenser is null ? null : MapCoinDispenser(response.CoinDispenser);
        int normalizedStatus = response.RequestStatus ?? 1;
        return new MachineStatus(normalizedStatus, recycler, hopper, dispenser, coinDispenser);
    }

    private static void EnsureAck(AcknowledgementResponse response)
        => EnsureAck(response.RequestType, response.RequestStatus, response.ErrorCode, response.Operation, response.OperationUser);

    private static void EnsureAck(VneRequestType requestType, int? reqStatus, int? errorCode, int? operation = null, string? operationUser = null)
    {
        int normalizedStatus = reqStatus ?? 1;
        if (normalizedStatus == 1)
        {
            return;
        }

        throw new VneApiException(requestType, errorCode, operation: operation, operationUser: operationUser);
    }

    private static int? MapNullableBoolean(bool? value)
    {
        return value.HasValue ? (value.Value ? 1 : 0) : null;
    }

    private static PaymentCancellationStatus MapCancellationStatus(int status)
        => status switch
        {
            1 => PaymentCancellationStatus.DeletedBeforeProcessing,
            2 => PaymentCancellationStatus.Pending,
            3 => PaymentCancellationStatus.NotCancellableAlreadyProcessed,
            4 => PaymentCancellationStatus.ReturnNotAvailable,
            _ => PaymentCancellationStatus.Pending
        };

    private static string? FormatDoorTimeout(TimeSpan? waitTimeout)
    {
        if (!waitTimeout.HasValue)
        {
            return "0";
        }

        TimeSpan span = waitTimeout.Value;
        if (span <= TimeSpan.Zero)
        {
            return "0";
        }

        if (span.Seconds == 0 && span.TotalMinutes <= 60)
        {
            int minutes = (int)Math.Clamp(span.TotalMinutes, 0, 60);
            return minutes.ToString();
        }

        if (span.TotalMinutes < 1)
        {
            int seconds = Math.Clamp(span.Seconds, 1, 59);
            return $"0:{seconds:00}";
        }

        throw new ArgumentOutOfRangeException(nameof(waitTimeout), "Allowed wait_timeout: 0-60 minutes or 0:01-0:59 seconds.");
    }

    private static MoneyCents ToMoney(int? value)
    {
        return new MoneyCents(value.GetValueOrDefault(0));
    }

    private static DateTimeOffset FromUnixSeconds(long? seconds)
        => seconds.HasValue ? DateTimeOffset.FromUnixTimeSeconds(seconds.Value) : DateTimeOffset.FromUnixTimeSeconds(0);

    private static (MoneyCents? value, bool restored) ParsePettyCash(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return (null, true);
        }

        bool restored = true;
        string digits = raw;
        if (digits.StartsWith("*", StringComparison.Ordinal))
        {
            restored = false;
            digits = digits[1..];
        }

        if (int.TryParse(digits, out int cents))
        {
            return (new MoneyCents(cents), restored);
        }

        return (null, restored);
    }

    private static TimelineRequestMessage BuildRangePayload(VneRequestType type, DateTimeOffset startDate, DateTimeOffset endDate, OperatorName? operatorName)
        => new(
            type,
            startDate.ToUnixTimeSeconds(),
            endDate.ToUnixTimeSeconds(),
            operatorName?.Value);

    private static (DateTimeOffset Start, DateTimeOffset End) RequireTimelineRange(DateTimeOffset? startDate, DateTimeOffset? endDate, string startParamName, string endParamName)
    {
        if (!startDate.HasValue)
        {
            throw new ArgumentNullException(startParamName, "Protocol requires start_date when requesting timeline reports.");
        }

        if (!endDate.HasValue)
        {
            throw new ArgumentNullException(endParamName, "Protocol requires end_date when requesting timeline reports.");
        }

        if (startDate.Value > endDate.Value)
        {
            throw new ArgumentException("startDate must be earlier than or equal to endDate.", startParamName);
        }

        return (startDate.Value, endDate.Value);
    }

    private static void EnsureMenuCredentials(int userLevel, OperatorName? operatorName, string? resolvedOperator, string? sanitizedPassword, string passwordParameterName)
    {
        if (userLevel is not 1 and not 2)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(resolvedOperator))
        {
            throw new ArgumentException("opName is required for userLevel 1 or 2 as per VNE protocol.", nameof(operatorName));
        }

        if (string.IsNullOrWhiteSpace(sanitizedPassword))
        {
            throw new ArgumentException("Password is required for userLevel 1 or 2 as per VNE protocol.", passwordParameterName);
        }
    }

    private static PaymentDetailStatus ParsePaymentDetailStatus(string status)
        => status?.ToLowerInvariant() switch
        {
            "completed" => PaymentDetailStatus.Completed,
            "returned" => PaymentDetailStatus.Returned,
            "partial" => PaymentDetailStatus.Partial,
            "deleted" => PaymentDetailStatus.Deleted,
            "pending" => PaymentDetailStatus.Pending,
            "notcompleted" => PaymentDetailStatus.NotCompleted,
            _ => PaymentDetailStatus.Pending
        };

    private static RecyclerStatus MapRecycler(RecyclerPayload payload)
    {
        List<CassetteStatus> cassettes = new();
        Dictionary<int, int>? stackerDetail = null;
        int? stacker = null;
        int? total = null;

        if (payload.ExtensionData is null)
        {
            return new RecyclerStatus(payload.Error, payload.Message, cassettes, stacker, total, stackerDetail);
        }

        foreach ((string key, JsonElement element) in payload.ExtensionData)
        {
            if (key.StartsWith("banconota_", StringComparison.OrdinalIgnoreCase))
            {
                CassetteStatus status = new(
                    GetInt32Flexible(element.GetProperty("valore")),
                    GetInt32Flexible(element.GetProperty("quantita")),
                    GetInt32Flexible(element.GetProperty("alert")),
                    element.TryGetProperty("min_level", out JsonElement min) ? GetInt32Flexible(min) : 0,
                    element.TryGetProperty("max_level", out JsonElement max) ? GetInt32Flexible(max) : 0);
                cassettes.Add(status);
            }
            else if (key.Equals("stacker", StringComparison.OrdinalIgnoreCase))
            {
                stacker = GetInt32Flexible(element);
            }
            else if (key.Equals("totalInRecycle", StringComparison.OrdinalIgnoreCase))
            {
                total = GetInt32Flexible(element);
            }
            else if (key.Equals("detailNotesStacker", StringComparison.OrdinalIgnoreCase))
            {
                stackerDetail = new();
                foreach (JsonProperty note in element.EnumerateObject())
                {
                    stackerDetail[int.Parse(note.Name)] = GetInt32Flexible(note.Value);
                }
            }
        }

        return new RecyclerStatus(payload.Error, payload.Message, cassettes, stacker, total, stackerDetail);
    }

    private static HopperStatus MapHopper(HopperPayload payload)
    {
        List<CoinStatus> coins = new();
        int? stacker = null;
        int? total = null;

        if (payload.ExtensionData is null)
        {
            return new HopperStatus(payload.Error, payload.Message, coins, stacker, total);
        }

        foreach ((string key, JsonElement element) in payload.ExtensionData)
        {
            if (key.StartsWith("moneta_", StringComparison.OrdinalIgnoreCase))
            {
                string[] fragments = key.Split('_', StringSplitOptions.RemoveEmptyEntries);
                if (fragments.Length == 2 && int.TryParse(fragments[1], out int denomination))
                {
                    CoinStatus coin = new(
                        denomination,
                        GetInt32Flexible(element),
                        payload.ExtensionData.TryGetValue(key + "_alert", out JsonElement alert) ? GetInt32Flexible(alert) : 0,
                        payload.ExtensionData.TryGetValue(key + "_min_level", out JsonElement min) ? GetInt32Flexible(min) : 0,
                        payload.ExtensionData.TryGetValue(key + "_max_level", out JsonElement max) ? GetInt32Flexible(max) : 0,
                        payload.ExtensionData.TryGetValue(key + "_accept", out JsonElement accept) && GetInt32Flexible(accept) == 1,
                        payload.ExtensionData.TryGetValue(key + "_recycle", out JsonElement recycle) && GetInt32Flexible(recycle) == 1,
                        payload.ExtensionData.TryGetValue(key + "_cashbox", out JsonElement cashbox) ? GetInt32Flexible(cashbox) : 0);
                    coins.Add(coin);
                }
            }
            else if (key.Equals("stacker", StringComparison.OrdinalIgnoreCase))
            {
                stacker = GetInt32Flexible(element);
            }
            else if (key.Equals("totalInRecycle", StringComparison.OrdinalIgnoreCase))
            {
                total = GetInt32Flexible(element);
            }
        }

        return new HopperStatus(payload.Error, payload.Message, coins, stacker, total);
    }

    private static DispenserStatus MapDispenser(DispenserPayload payload)
    {
        List<DispenserCassetteStatus> cassettes = new();
        int? total = null;
        int? reject = null;

        if (payload.ExtensionData is not null)
        {
            foreach ((string key, JsonElement element) in payload.ExtensionData)
            {
                if (key.StartsWith("banconota_", StringComparison.OrdinalIgnoreCase))
                {
                    DispenserCassetteStatus cassette = new(
                        GetInt32Flexible(element.GetProperty("valore")),
                        GetInt32Flexible(element.GetProperty("quantita")),
                        GetInt32Flexible(element.GetProperty("alert")),
                        element.TryGetProperty("error", out JsonElement error) ? GetInt32Flexible(error) : 0);
                    cassettes.Add(cassette);
                }
                else if (key.Equals("totalInRecycle", StringComparison.OrdinalIgnoreCase))
                {
                    total = GetInt32Flexible(element);
                }
                else if (key.Equals("rejectBin", StringComparison.OrdinalIgnoreCase))
                {
                    reject = GetInt32Flexible(element);
                }
            }
        }

        return new DispenserStatus(payload.Error, payload.Message, cassettes, total, reject);
    }

    private static CoinDispenserStatus MapCoinDispenser(CoinDispenserPayload payload)
    {
        List<CoinDispenserHopperStatus> hoppers = new();

        if (payload.ExtensionData is not null)
        {
            foreach ((string key, JsonElement element) in payload.ExtensionData)
            {
                if (key.StartsWith("hopper_", StringComparison.OrdinalIgnoreCase))
                {
                    string[] fragments = key.Split('_', StringSplitOptions.RemoveEmptyEntries);
                    if (fragments.Length == 2 && int.TryParse(fragments[1], out int index))
                    {
                        CoinDispenserHopperStatus hopper = new(
                            index,
                            element.TryGetProperty("valore", out JsonElement value) ? GetInt32Flexible(value) : 0,
                            element.TryGetProperty("quantita", out JsonElement quantity) ? GetInt32Flexible(quantity) : 0,
                            element.TryGetProperty("alert", out JsonElement alert) ? GetInt32Flexible(alert) : 0,
                            element.TryGetProperty("error", out JsonElement error) ? GetInt32Flexible(error) : 0);
                        hoppers.Add(hopper);
                    }
                }
            }
        }

        IReadOnlyList<CoinDispenserHopperStatus> ordered = hoppers.OrderBy(h => h.Index).ToArray();
        return new CoinDispenserStatus(ordered);
    }

    private static int GetInt32Flexible(JsonElement element)
        => element.ValueKind switch
        {
            JsonValueKind.Number => element.TryGetInt32(out int value) ? value : (int)element.GetInt64(),
            JsonValueKind.String when int.TryParse(element.GetString(), out int parsed) => parsed,
            JsonValueKind.Null => 0,
            _ => throw new JsonException($"The requested operation requires an element of type 'Number', but the target element has type '{element.ValueKind}'.")
        };

    private static int ExtractListIndex(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return int.MaxValue;
        }

        int underscoreIndex = key.LastIndexOf('_');
        if (underscoreIndex >= 0 && underscoreIndex < key.Length - 1 &&
            int.TryParse(key[(underscoreIndex + 1)..], out int index))
        {
            return index;
        }

        return int.MaxValue;
    }
}
