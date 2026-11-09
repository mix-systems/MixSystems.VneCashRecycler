using System.Text.Json.Serialization;
using MixSystems.VneCashRecycler.Models;

namespace MixSystems.VneCashRecycler.Protocol.Requests;

internal sealed record StartRefillRequestMessage(
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("opName")] string? OperatorName,
    [property: JsonPropertyName("acceptAll")] int? AcceptAll);

internal sealed record EndRefillRequestMessage(
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("opName")] string? OperatorName);

internal sealed record ManualRefillRequestMessage(
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("opName")] string? OperatorName,
    [property: JsonPropertyName("refill")] IReadOnlyDictionary<string, int> RefillValues);

internal sealed record RefillStatusRequestMessage(
    [property: JsonPropertyName("tipo")] VneRequestType RequestType);
