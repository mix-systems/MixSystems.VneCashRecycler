using MixSystems.VneCashRecycler.ValueObjects;

namespace MixSystems.VneCashRecycler.Configuration;

/// <summary>
/// Per-client configuration describing where and how to reach a specific recycler.
/// </summary>
public sealed class VneCashRecyclerClientOptions
{
    /// <summary>
    /// Absolute endpoint URL (e.g. https://192.168.1.6/selfcashapi/).
    /// </summary>
    public required Uri Endpoint { get; init; }
        = new("https://127.0.0.1/selfcashapi/");

    /// <summary>
    /// Optional default operator name (opName) automatically injected if request does not override it.
    /// </summary>
    public OperatorName? DefaultOperatorName { get; init; }
        = default;

    public VneResilienceOptions Resilience { get; init; } = new();

    public VneCertificateOptions Certificate { get; init; } = new();

    /// <summary>
    /// Returns the endpoint string for debugging purposes.
    /// </summary>
    public override string ToString() => Endpoint.ToString();
}
