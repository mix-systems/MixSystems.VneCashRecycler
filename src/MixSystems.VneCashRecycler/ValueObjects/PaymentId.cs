namespace MixSystems.VneCashRecycler.ValueObjects;

/// <summary>
/// Wraps the identifier returned by the VNE machine for a pending payment.
/// </summary>
public readonly record struct PaymentId
{
    /// <summary>
    /// Initializes a strongly-typed payment identifier.
    /// </summary>
    public PaymentId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value.Trim();
    }

    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;
}
