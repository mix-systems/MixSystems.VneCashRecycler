namespace MixSystems.VneCashRecycler.ValueObjects;

/// <summary>
/// Represents the optional operator name passed to the VNE API (opName field).
/// </summary>
public readonly record struct OperatorName
{
    /// <summary>
    /// Initializes an operator name wrapper for the opName field.
    /// </summary>
    public OperatorName(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Value = value.Trim();
    }

    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;
}
