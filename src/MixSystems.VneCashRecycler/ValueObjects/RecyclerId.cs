namespace MixSystems.VneCashRecycler.ValueObjects;

/// <summary>
/// Strongly typed identifier for a physical VNE recycler (managed by the application layer).
/// </summary>
public readonly record struct RecyclerId
{
    /// <summary>
    /// Initializes a new identifier for a physical recycler.
    /// </summary>
    public RecyclerId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value.Trim();
    }

    public string Value { get; }

    /// <summary>
    /// Returns the identifier string.
    /// </summary>
    public override string ToString() => Value;
}
