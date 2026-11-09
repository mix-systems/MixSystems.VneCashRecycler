namespace MixSystems.VneCashRecycler.ValueObjects;

/// <summary>
/// Represents a monetary amount expressed in euro cents (to match the VNE protocol).
/// </summary>
public readonly record struct MoneyCents
{
    /// <summary>
    /// Initializes a monetary amount expressed in cents.
    /// </summary>
    public MoneyCents(long value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);
        Value = value;
    }

    public long Value { get; }

    /// <summary>
    /// Converts the amount to decimal euros.
    /// </summary>
    public decimal ToDecimal() => Value / 100m;

    /// <summary>
    /// Creates a <see cref="MoneyCents"/> from a decimal Euro value.
    /// </summary>
    public static MoneyCents FromDecimal(decimal amount)
    {
        decimal rounded = Math.Round(amount * 100m, MidpointRounding.AwayFromZero);
        if (rounded is < 0 or > long.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be between 0 and 92,233,720,368,547,758.07");
        }

        return new MoneyCents((long)rounded);
    }

    /// <inheritdoc />
    public override string ToString() => Value.ToString();

    /// <summary>
    /// Explicitly converts to <see cref="int"/> with overflow checking.
    /// </summary>
    public static explicit operator int(MoneyCents cents) => checked((int)cents.Value);
}
