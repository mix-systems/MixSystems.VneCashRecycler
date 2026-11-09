namespace MixSystems.VneCashRecycler.Models;

/// <summary>
/// Optional knobs for payment creation (refundable flag, card usage, authentication key).
/// Maps directly to the optional fields defined in the VNE protocol.
/// </summary>
public sealed record PaymentRequestOptions
{
    /// <summary>
    /// When set, overrides the default refundability of the payment (1 = refundable, 0 = not refundable).
    /// </summary>
    public bool? IsRefundable { get; init; }

    /// <summary>
    /// When set, forces the payment to be managed by the card module (1) or cash module (0).
    /// </summary>
    public bool? UseCreditCard { get; init; }

    /// <summary>
    /// Authentication key required by specific installations (authKey field).
    /// </summary>
    public string? AuthenticationKey { get; init; }

}
