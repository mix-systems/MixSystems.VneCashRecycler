using Microsoft.Extensions.DependencyInjection;
using MixSystems.VneCashRecycler.Clients;
using MixSystems.VneCashRecycler.Extensions;
using MixSystems.VneCashRecycler.Serialization;

namespace MixSystems.VneCashRecycler.UnitTests.Configuration;

public class ServiceCollectionExtensionsTests
{
    [Trait("Category", "Unit")]
    [Fact]
    public void AddVneCashRecycler_RegistersFactoryAndSerializerAccessor()
    {
        ServiceCollection services = new();
        services.AddLogging();

        services.AddVneCashRecycler();

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        Assert.NotNull(serviceProvider.GetRequiredService<IVneCashRecyclerClientFactory>());
        Assert.NotNull(serviceProvider.GetRequiredService<VneJsonSerializerOptionsAccessor>());
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void AddVneCashRecycler_AppliesSerializerCustomization()
    {
        ServiceCollection services = new();
        services.AddLogging();

        services.AddVneCashRecycler(options => options.WriteIndented = true);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        VneJsonSerializerOptionsAccessor accessor = serviceProvider.GetRequiredService<VneJsonSerializerOptionsAccessor>();

        Assert.True(accessor.Options.WriteIndented);
        Assert.Contains(accessor.Options.Converters, converter => converter is VneRequestTypeJsonConverter);
    }
}
