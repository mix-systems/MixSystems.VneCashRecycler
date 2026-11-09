namespace MixSystems.VneCashRecycler.ValueObjects;

/// <summary>
/// Strongly typed identifier for operator withdrawal requests.
/// </summary>
public readonly record struct WithdrawalId
{
    /// <summary>
    /// Initializes a strongly-typed identifier for a withdrawal operation.
    /// </summary>
    public WithdrawalId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value.Trim();
    }

    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;
}
