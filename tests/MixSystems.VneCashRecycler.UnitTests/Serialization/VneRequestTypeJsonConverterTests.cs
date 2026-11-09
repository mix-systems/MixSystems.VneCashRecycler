using System.Text.Json;
using MixSystems.VneCashRecycler.Models;
using MixSystems.VneCashRecycler.Serialization;

namespace MixSystems.VneCashRecycler.UnitTests.Serialization;

public class VneRequestTypeJsonConverterTests
{
    [Trait("Category", "Unit")]
    [Fact]
    public void Read_ReturnsEnumForKnownNumericValue()
    {
        JsonSerializerOptions options = CreateOptions();

        VneRequestType value = JsonSerializer.Deserialize<VneRequestType>("1", options);

        Assert.Equal(VneRequestType.PaymentRequest, value);
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void Read_ThrowsForNonNumericToken()
    {
        JsonSerializerOptions options = CreateOptions();

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<VneRequestType>("\"1\"", options));
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void Read_ThrowsForUnknownNumericValue()
    {
        JsonSerializerOptions options = CreateOptions();

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<VneRequestType>("999", options));
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void Write_SerializesEnumAsNumericValue()
    {
        JsonSerializerOptions options = CreateOptions();

        string json = JsonSerializer.Serialize(VneRequestType.MachineStatus, options);

        Assert.Equal("20", json);
    }

    private static JsonSerializerOptions CreateOptions()
    {
        JsonSerializerOptions options = new();
        options.Converters.Add(new VneRequestTypeJsonConverter());
        return options;
    }
}
