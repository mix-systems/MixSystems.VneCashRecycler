using System.Text.Json;
using System.Text.Json.Serialization;
using MixSystems.VneCashRecycler.Models;

namespace MixSystems.VneCashRecycler.Serialization;

internal sealed class VneRequestTypeJsonConverter : JsonConverter<VneRequestType>
{
    /// <summary>
    /// Reads the numeric "tipo" value from JSON and maps it to <see cref="VneRequestType"/>.
    /// </summary>
    public override VneRequestType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number)
        {
            throw new JsonException("Request type must be numeric");
        }

        int value = reader.GetInt32();
        if (Enum.IsDefined(typeof(VneRequestType), value))
        {
            return (VneRequestType)value;
        }

        throw new JsonException($"Unknown VNE request type value {value}");
    }

    /// <summary>
    /// Writes the enum value back as the numeric "tipo" field.
    /// </summary>
    public override void Write(Utf8JsonWriter writer, VneRequestType value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue((int)value);
    }
}
