namespace MixSystems.VneCashRecycler.Protocol;

internal static class VneErrorCatalog
{
    private static readonly IReadOnlyDictionary<int, string> Descriptions =
        new Dictionary<int, string>
        {
            [100] = "Generic error in the request package structure. Check whether the payload is correct.",
            [101] = "Unknown parameter 'tipo' in request package. Verify the command type.",
            [102] = "Generic error managing the request. Contact VNE support.",
            [103] = "Error managing the payment or withdrawal request. Retry, then contact VNE if it persists.",
            [104] = "Unknown parameter 'id'. Ensure the transaction identifier matches a previous response.",
            [105] = "Cannot accept new payment because another payment is already in progress.",
            [106] = "Cannot perform withdrawal: denomination passed is not present or available.",
            [107] = "Cannot perform logout because a payment is still in progress.",
            [108] = "Username/password do not correspond to a machine user. Check credentials.",
            [110] = "Cannot process new request: another operation is already in progress.",
            [113] = "Error during cancel request for a POS payment. POS operation cannot be cancelled.",
            [114] = "Cannot cancel pending payment because a different user initiated it (unless super-user).",
            [120] = "Authentication failed. Validate the authentication string/opName configuration.",
            [165] = "Refund not available: either the payment cannot be refunded or it has already been fully refunded."
        };

    /// <summary>
    /// Returns the textual error description from Appendix I for the specified VNE error code.
    /// </summary>
    public static string? GetDescription(int? code)
        => code.HasValue && Descriptions.TryGetValue(code.Value, out string? description)
            ? description
            : null;
}
