using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MixSystems.VneCashRecycler.Clients;
using MixSystems.VneCashRecycler.Http;
using MixSystems.VneCashRecycler.Serialization;

namespace MixSystems.VneCashRecycler.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the reusable components required to create VNE recycler clients.
    /// Application code can then use <see cref="IVneCashRecyclerClientFactory"/> to instantiate
    /// clients with specific endpoints/configurations.
    /// </summary>
    /// <param name="services">Service collection to extend.</param>
    /// <param name="configureSerializer">Optional callback to customize JSON serialization used for protocol payloads.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddVneCashRecycler(
        this IServiceCollection services,
        Action<JsonSerializerOptions>? configureSerializer = null)
    {
        services.TryAddSingleton(_ => new VneJsonSerializerOptionsAccessor(configureSerializer));

        services.AddSingleton<VneHttpRequestExecutor>();
        services.AddSingleton<IVneCashRecyclerClientFactory, VneCashRecyclerClientFactory>();

        return services;
    }
}
