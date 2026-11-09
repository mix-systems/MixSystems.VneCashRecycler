using MixSystems.VneCashRecycler.IntegrationTests.Fixtures;
using MixSystems.VneCashRecycler.Models;

namespace MixSystems.VneCashRecycler.IntegrationTests.Tests;

[Collection("VNE-Recycler-tests")]
[Trait("Category", "Integration")]
public sealed class MachineStatusTests
{
    private readonly VneRecyclerClientFixture _fixture;

    public MachineStatusTests(VneRecyclerClientFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Machine_status_returns_ack()
    {
        MachineStatus status = await _fixture.Client.GetMachineStatusAsync();
        Assert.Equal(1, status.RequestStatus);
    }

    [Fact]
    public async Task Machine_status_parses_variable_banknote_and_coin_tags()
    {
        MachineStatus status = await _fixture.Client.GetMachineStatusAsync();

        Assert.NotNull(status.Recycler);
        Assert.NotNull(status.Hopper);
        Assert.NotNull(status.Dispenser);
        Assert.NotNull(status.CoinDispenser);

        Assert.Equal(4, status.Recycler!.Cassettes.Count);
        Assert.Equal(new[] { 500, 1000, 2000, 5000 }, status.Recycler.Cassettes.Select(c => c.Denomination).ToArray());
        Assert.Equal(4, status.Recycler.StackerDetail!.Count);
        Assert.Equal(5, status.Hopper!.Coins.Count);
        Assert.Contains(status.Hopper.Coins, coin => coin.Denomination == 50);
        Assert.Equal(2, status.CoinDispenser!.Hoppers.Count);
        Assert.Equal(2, status.Dispenser!.Cassettes.Count);
    }
}
