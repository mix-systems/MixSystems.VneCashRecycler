namespace MixSystems.VneCashRecycler.ValueObjects;

/// <summary>
/// Represents the optional legacy "operatore" field supported by the VNE protocol.
/// Although deprecated in recent firmware versions, some legacy machines still require it.
/// </summary>
public readonly record struct OperatorIdentifier
{
    /// <summary>
    /// Initializes the identifier used for the deprecated "operatore" payload field.
    /// </summary>
    public OperatorIdentifier(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Value = value.Trim();
    }

    /// <summary>
    /// Raw identifier passed to the machine (can be empty for legacy super-user scenarios).
    /// </summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;
}
