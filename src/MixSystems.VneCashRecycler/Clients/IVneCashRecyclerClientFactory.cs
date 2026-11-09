using MixSystems.VneCashRecycler.Configuration;

namespace MixSystems.VneCashRecycler.Clients;

/// <summary>
/// Factory for building typed VNE recycler clients bound to a specific endpoint/configuration.
/// </summary>
public interface IVneCashRecyclerClientFactory
{
    /// <summary>
    /// Creates a client instance configured with the provided endpoint, certificate and resilience options.
    /// </summary>
    IVneCashRecyclerClient Create(VneCashRecyclerClientOptions options);
}
