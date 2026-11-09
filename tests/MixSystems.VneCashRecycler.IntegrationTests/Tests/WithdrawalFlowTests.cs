using MixSystems.VneCashRecycler.IntegrationTests.Fixtures;
using MixSystems.VneCashRecycler.Exceptions;
using MixSystems.VneCashRecycler.Models;
using MixSystems.VneCashRecycler.ValueObjects;

namespace MixSystems.VneCashRecycler.IntegrationTests.Tests;

[Collection("VNE-Recycler-tests")]
[Trait("Category", "Integration")]
public class WithdrawalFlowTests
{
    private readonly VneRecyclerClientFixture _fixture;

    public WithdrawalFlowTests(VneRecyclerClientFixture fixture)
    {
        _fixture = fixture;
    }

[Fact(Skip = "Requires recycler configured with sufficient cash for operator withdrawal.")]
public async Task ManualPolling_returns_final_status_for_withdrawal()
    {
        MoneyCents amount = new(100);
        WithdrawalRequestResult? request = null;

        try
        {
            request = await StartWithdrawalWithRetryAsync(amount);

            WithdrawalProgress progress = await _fixture.WaitUntilWithdrawalCompletesAsync(
                request.WithdrawalId,
                cancellationToken: default);

            Assert.Equal(WithdrawalStatus.Completed, progress.Status);
            Assert.True(progress.WithdrawnAmount.Value >= 0);
        }
        finally
        {
            await _fixture.Client.EndWithdrawalAsync();
        }
    }

    private async Task<WithdrawalRequestResult> StartWithdrawalWithRetryAsync(MoneyCents amount)
    {
        const int maxAttempts = 5;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            try
            {
                return await _fixture.Client.StartWithdrawalAsync(amount);
            }
            catch (VneApiException ex) when (ex.ErrorCode is 103 or 106)
            {
                await Task.Delay(500);
            }
        }

        throw new TimeoutException("Unable to start withdrawal due to repeated 103/106 errors. Ensure machine has enough cash in the recycler.");
    }
}
