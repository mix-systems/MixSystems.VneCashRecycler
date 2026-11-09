using MixSystems.VneCashRecycler.IntegrationTests.Fixtures;
using MixSystems.VneCashRecycler.Exceptions;
using MixSystems.VneCashRecycler.Models;
using MixSystems.VneCashRecycler.ValueObjects;

namespace MixSystems.VneCashRecycler.IntegrationTests.Tests;

[Collection("VNE-Recycler-tests")]
[Trait("Category", "Integration")]
public class ErrorHandlingTests
{
    private readonly VneRecyclerClientFixture _fixture;

    public ErrorHandlingTests(VneRecyclerClientFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Duplicate_payment_returns_error_105()
    {
        await _fixture.EnsureNoPendingPaymentsAsync();

        PaymentRequestResult first = await _fixture.Client.SendPaymentAsync(new MoneyCents(300));
        try
        {
            VneApiException ex = await Assert.ThrowsAsync<VneApiException>(() => _fixture.Client.SendPaymentAsync(new MoneyCents(200)));
            Assert.Equal(105, ex.ErrorCode);
        }
        finally
        {
            await _fixture.Client.CancelPaymentAsync(first.PaymentId, PaymentCancellationMode.ReturnInsertedAmount);
            await _fixture.WaitUntilFinalStateAsync(first.PaymentId);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Pending_list_empty_when_no_active_payments()
    {
        await _fixture.EnsureNoPendingPaymentsAsync();

        PendingPaymentList list = await _fixture.Client.ListPendingPaymentsAsync();
        Assert.Empty(list.PaymentIds);
    }
}
