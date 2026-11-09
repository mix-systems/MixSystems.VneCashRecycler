using System.Text.Json;
using System.Text.Json.Serialization;

namespace MixSystems.VneCashRecycler.Serialization;

/// <summary>
/// Provides the JSON serialization settings required by the VNE protocol implementation.
/// Encapsulating the options prevents other libraries from overwriting converters in DI.
/// </summary>
internal sealed class VneJsonSerializerOptionsAccessor
{
    public JsonSerializerOptions Options { get; }

    public VneJsonSerializerOptionsAccessor(Action<JsonSerializerOptions>? configureSerializer)
    {
        JsonSerializerOptions options = new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        options.Converters.Add(new VneRequestTypeJsonConverter());
        configureSerializer?.Invoke(options);

        Options = options;
    }
}
