using MixSystems.VneCashRecycler.IntegrationTests.Fixtures;
using MixSystems.VneCashRecycler.Exceptions;
using MixSystems.VneCashRecycler.Models;
using MixSystems.VneCashRecycler.ValueObjects;

namespace MixSystems.VneCashRecycler.IntegrationTests.Tests;

[Collection("VNE-Recycler-tests")]
[Trait("Category", "Integration")]
public class PaymentFlowTests
{
    private readonly VneRecyclerClientFixture _fixture;

    public PaymentFlowTests(VneRecyclerClientFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Payment_lifecycle_supports_polling_and_cancellation()
    {
        await _fixture.EnsureNoPendingPaymentsAsync();

        MoneyCents amount = new(500);
        PaymentRequestResult request = await SendPaymentWithRetryAsync(amount);
        Assert.NotNull(request);

        PendingPaymentList pendingList = await _fixture.Client.ListPendingPaymentsAsync();
        Assert.Contains(request.PaymentId, pendingList.PaymentIds);

        PaymentProgress initial = await _fixture.Client.GetPaymentStatusAsync(request.PaymentId);
        Assert.Equal(PaymentLifecycleStatus.Pending, initial.LifecycleStatus);

        await _fixture.Client.CancelPaymentAsync(request.PaymentId, PaymentCancellationMode.ReturnInsertedAmount);

        PaymentProgress final = await _fixture.WaitUntilFinalStateAsync(request.PaymentId);
        Assert.Equal(PaymentLifecycleStatus.Completed, final.LifecycleStatus);
        Assert.Contains(final.DetailStatus, new[]
        {
            PaymentDetailStatus.Returned,
            PaymentDetailStatus.Partial,
            PaymentDetailStatus.Deleted
        });
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task ManualPolling_returns_final_status_after_cancellation()
    {
        await _fixture.EnsureNoPendingPaymentsAsync();

        MoneyCents amount = new(500);
        PaymentRequestResult request = await SendPaymentWithRetryAsync(amount);

        await _fixture.Client.CancelPaymentAsync(request.PaymentId, PaymentCancellationMode.ReturnInsertedAmount);

        PaymentProgress result = await _fixture.WaitUntilFinalStateAsync(request.PaymentId);

        Assert.Equal(PaymentLifecycleStatus.Completed, result.LifecycleStatus);
        Assert.True(result.DetailStatus is PaymentDetailStatus.Returned or PaymentDetailStatus.Partial or PaymentDetailStatus.Deleted);
    }

    private async Task<PaymentRequestResult> SendPaymentWithRetryAsync(MoneyCents amount)
    {
        const int maxAttempts = 5;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            try
            {
                return await _fixture.Client.SendPaymentAsync(amount);
            }
            catch (VneApiException ex) when (ex.ErrorCode == 105)
            {
                await Task.Delay(500);
            }
        }

        throw new TimeoutException("Unable to start payment due to continuous single-payment lock.");
    }
}
