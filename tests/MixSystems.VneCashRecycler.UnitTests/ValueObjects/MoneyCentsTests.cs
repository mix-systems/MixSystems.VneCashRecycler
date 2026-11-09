using MixSystems.VneCashRecycler.ValueObjects;

namespace MixSystems.VneCashRecycler.UnitTests.ValueObjects;

public class MoneyCentsTests
{
    [Trait("Category", "Unit")]
    [Fact]
    public void Constructor_RejectsNegativeValues()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new MoneyCents(-1));
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void FromDecimal_RoundsAwayFromZero()
    {
        MoneyCents value = MoneyCents.FromDecimal(12.345m);

        Assert.Equal(1235, value.Value);
        Assert.Equal(12.35m, value.ToDecimal());
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void FromDecimal_RejectsNegativeAmounts()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MoneyCents.FromDecimal(-0.01m));
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void ExplicitIntConversion_ChecksOverflow()
    {
        MoneyCents value = new((long)int.MaxValue + 1);

        Assert.Throws<OverflowException>(() => _ = (int)value);
    }
}
