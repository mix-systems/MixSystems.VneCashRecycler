using MixSystems.VneCashRecycler.Models;
using MixSystems.VneCashRecycler.ValueObjects;

namespace MixSystems.VneCashRecycler.Clients;

/// <summary>
/// Defines the commands exposed by the VNE Automatic Cash API as described in
/// VNE_RETAIL_communication_protocol_3_04-ENG (April 21, 2023).
/// </summary>
public interface IVneCashRecyclerClient : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// Starts a payment request (command tipo = 1) for the specified amount and optional operator context.
    /// </summary>
    /// <param name="operatorIdentifier">
    /// Optional legacy “operatore” identifier (deprecated in protocol v3.04, still required by some legacy machines).
    /// Leave <c>null</c> unless a specific installation demands it.
    /// </param>
    Task<PaymentRequestResult> SendPaymentAsync(
        MoneyCents amount,
        OperatorName? operatorName = null,
        CancellationToken cancellationToken = default,
        PaymentRequestOptions? options = null,
        OperatorIdentifier? operatorIdentifier = null);

    /// <summary>
    /// Retrieves the current status snapshot of a payment (command tipo = 2).
    /// </summary>
    Task<PaymentProgress> GetPaymentStatusAsync(PaymentId paymentId, OperatorName? operatorName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a pending payment (command tipo = 3) with the requested cancellation mode.
    /// </summary>
    Task<PaymentCancellationResult> CancelPaymentAsync(PaymentId paymentId, PaymentCancellationMode cancellationMode, OperatorName? operatorName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves identifiers of all pending payments (command tipo = 5).
    /// </summary>
    Task<PendingPaymentList> ListPendingPaymentsAsync(OperatorName? operatorName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries the machine status (command tipo = 20) including recycler, hopper and dispenser telemetry.
    /// </summary>
    Task<MachineStatus> GetMachineStatusAsync(OperatorName? operatorName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Switches the recycler into refill mode (command tipo = 30).
    /// </summary>
    Task StartRefillAsync(bool acceptAll = false, OperatorName? operatorName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ends refill mode and returns to payment mode (command tipo = 31).
    /// </summary>
    Task EndRefillAsync(OperatorName? operatorName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a manual refill for selected denominations (command tipo = 32).
    /// </summary>
    Task ManualRefillAsync(ManualRefillRequest request, OperatorName? operatorName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Polls the ongoing refill operation (command tipo = 33) to obtain totals.
    /// </summary>
    Task<RefillPollingResult> PollRefillAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs an operator into the machine (command tipo = 40).
    /// </summary>
    Task LoginOperatorAsync(OperatorName? operatorName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs the operator out of the machine (command tipo = 41).
    /// </summary>
    Task LogoutOperatorAsync(OperatorName? operatorName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts an operator withdrawal (command tipo = 10) with an optional payout preference.
    /// </summary>
    /// <param name="operatorIdentifier">
    /// Optional legacy “operatore” identifier used by legacy firmware to tie the request to a terminal. Leave <c>null</c> otherwise.
    /// </param>
    Task<WithdrawalRequestResult> StartWithdrawalAsync(
        MoneyCents amount,
        WithdrawalDenomination? payoutMode = null,
        OperatorName? operatorName = null,
        CancellationToken cancellationToken = default,
        OperatorIdentifier? operatorIdentifier = null);

    /// <summary>
    /// Retrieves the current status snapshot of a withdrawal (command tipo = 11).
    /// </summary>
    Task<WithdrawalProgress> GetWithdrawalStatusAsync(WithdrawalId withdrawalId, OperatorName? operatorName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ends withdrawal mode (command tipo = 12).
    /// </summary>
    Task EndWithdrawalAsync(OperatorName? operatorName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Triggers hopper emptying (command tipo = 50).
    /// </summary>
    Task StartHopperEmptyingAsync(bool emptyToPettyCash = false, OperatorName? operatorName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Triggers recycler emptying (command tipo = 51).
    /// </summary>
    Task StartRecyclerEmptyingAsync(bool emptyToPettyCash = false, OperatorName? operatorName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets the stacker or cashbox counters (command tipo = 52).
    /// </summary>
    Task ResetStackerAsync(StackerPeripheral peripheral, OperatorName? operatorName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Polls the status of an emptying operation (command tipo = 53).
    /// </summary>
    Task<EmptyingStatusResult> PollEmptyingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts single-denomination hopper emptying (command tipo = 54).
    /// </summary>
    Task StartCoinDispenserEmptyingAsync(CoinDispenserSelection selection, OperatorName? operatorName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens the electronic lock (command tipo = 55) with optional delay or secondary port.
    /// </summary>
    Task OpenDoorAsync(TimeSpan? waitTimeout = null, int? port = null, OperatorName? operatorName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Requests a cash closing snapshot (command tipo = 60).
    /// </summary>
    Task<CashClosingReport> GetCashClosingAsync(OperatorName? operatorName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts a refund for a completed or partial payment (command tipo = 65).
    /// </summary>
    Task StartRefundAsync(PaymentId paymentId, OperatorName? operatorName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Polls refund delivery status (command tipo = 66).
    /// </summary>
    Task<RefundProgress> PollRefundAsync(PaymentId paymentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves withdrawals list within a time window (command tipo = 70). Both <paramref name="startDate"/> and <paramref name="endDate"/> are mandatory per protocol and must be expressed as UNIX timestamps.
    /// </summary>
    Task<WithdrawalReport> GetWithdrawalsAsync(DateTimeOffset? startDate, DateTimeOffset? endDate, OperatorName? operatorName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves payments list within a time window (command tipo = 71). Both <paramref name="startDate"/> and <paramref name="endDate"/> are mandatory per protocol and must be expressed as UNIX timestamps.
    /// </summary>
    Task<PaymentReport> GetPaymentsAsync(DateTimeOffset? startDate, DateTimeOffset? endDate, OperatorName? operatorName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves door opening events (command tipo = 72). Both <paramref name="startDate"/> and <paramref name="endDate"/> are mandatory per protocol and must be expressed as UNIX timestamps.
    /// </summary>
    Task<OpeningReport> GetOpeningsAsync(DateTimeOffset? startDate, DateTimeOffset? endDate, OperatorName? operatorName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the history of cash closings (command tipo = 73). Both <paramref name="startDate"/> and <paramref name="endDate"/> are mandatory per protocol and must be expressed as UNIX timestamps.
    /// </summary>
    Task<CashClosingHistoryReport> GetCashClosingHistoryAsync(DateTimeOffset? startDate, DateTimeOffset? endDate, OperatorName? operatorName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens the configuration menu with the required level (command tipo = 80). For <paramref name="userLevel"/> 1 (refill) or 2 (admin) both <paramref name="operatorName"/> and <paramref name="password"/> must be provided so they match an existing user on the machine.
    /// </summary>
    Task AccessConfigurationMenuAsync(int userLevel, OperatorName? operatorName = null, string? password = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reboots or shuts down the machine remotely (command tipo = 81).
    /// </summary>
    Task RebootOrShutdownAsync(bool restart, OperatorName? operatorName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries the software version of the machine (command tipo = 82).
    /// </summary>
    Task<string?> GetSoftwareVersionAsync(OperatorName? operatorName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates peripheral configuration such as cassette levels (command tipo = 90).
    /// </summary>
    Task ConfigurePeripheralsAsync(PeripheralsConfiguration configuration, OperatorName? operatorName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates software configuration flags (command tipo = 91).
    /// </summary>
    Task ConfigureSoftwareAsync(SoftwareConfiguration configuration, OperatorName? operatorName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the petty cash amount (command tipo = 92) and returns the resulting status snapshot.
    /// </summary>
    Task<MachineStatus> SetPettyCashAsync(MoneyCents pettyCash, OperatorName? operatorName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Signals the completion of a petty cash restore procedure (command tipo = 93).
    /// </summary>
    Task SignalEndPettyCashRestoreAsync(CancellationToken cancellationToken = default);
}
