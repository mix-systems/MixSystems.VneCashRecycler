using MixSystems.VneCashRecycler.IntegrationTests.Fixtures;
using MixSystems.VneCashRecycler.ValueObjects;

namespace MixSystems.VneCashRecycler.IntegrationTests.Tests;

public sealed class ClientValidationTests : IClassFixture<VneRecyclerClientFixture>
{
    private readonly VneRecyclerClientFixture _fixture;

    public ClientValidationTests(VneRecyclerClientFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetPaymentsAsync_ThrowsWhenStartDateMissing()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _fixture.Client.GetPaymentsAsync(null, DateTimeOffset.UtcNow));
    }

    [Fact]
    public async Task GetPaymentsAsync_ThrowsWhenEndDateMissing()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _fixture.Client.GetPaymentsAsync(now, null));
    }

    [Fact]
    public async Task GetPaymentsAsync_ThrowsWhenStartAfterEnd()
    {
        DateTimeOffset start = DateTimeOffset.UtcNow;
        DateTimeOffset end = start.AddMinutes(-1);

        await Assert.ThrowsAsync<ArgumentException>(
            () => _fixture.Client.GetPaymentsAsync(start, end));
    }

    [Fact]
    public async Task AccessConfigurationMenuAsync_Level1RequiresPassword()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _fixture.Client.AccessConfigurationMenuAsync(1, new OperatorName("refill"), password: null));
    }

    [Fact]
    public async Task AccessConfigurationMenuAsync_Level2RequiresOperator()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _fixture.Client.AccessConfigurationMenuAsync(2, operatorName: null, password: "admin"));
    }
}
