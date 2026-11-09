using System.Text.Json;
using System.Text.Json.Serialization;
using MixSystems.VneCashRecycler.Models;

namespace MixSystems.VneCashRecycler.Protocol.Responses;

internal sealed record MachineStatusResponse(
    [property: JsonPropertyName("req_status")] int? RequestStatus,
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("recycler")] RecyclerPayload? Recycler,
    [property: JsonPropertyName("hopper")] HopperPayload? Hopper,
    [property: JsonPropertyName("dispenser")] DispenserPayload? Dispenser,
    [property: JsonPropertyName("coinDisp")] CoinDispenserPayload? CoinDispenser,
    [property: JsonPropertyName("mess")] int? ErrorCode,
    [property: JsonPropertyName("operation")] int? Operation,
    [property: JsonPropertyName("user")] string? OperationUser);

internal sealed record RecyclerPayload(
    [property: JsonPropertyName("error")] int Error,
    [property: JsonPropertyName("mess")] string Message)
{
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; init; } = new();
}

internal sealed record HopperPayload(
    [property: JsonPropertyName("error")] int Error,
    [property: JsonPropertyName("mess")] string Message)
{
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; init; } = new();
}

internal sealed record DispenserPayload(
    [property: JsonPropertyName("error")] int Error,
    [property: JsonPropertyName("mess")] string Message)
{
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; init; } = new();
}

internal sealed record CoinDispenserPayload
{
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; init; } = new();
}
