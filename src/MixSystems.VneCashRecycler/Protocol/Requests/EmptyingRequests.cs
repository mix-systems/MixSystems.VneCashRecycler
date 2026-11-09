using System.Text.Json.Serialization;
using MixSystems.VneCashRecycler.Models;

namespace MixSystems.VneCashRecycler.Protocol.Requests;

internal sealed record HopperEmptyingRequestMessage(
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("opName")] string? OperatorName,
    [property: JsonPropertyName("full")] int? EmptyToPettyCash);

internal sealed record RecyclerEmptyingRequestMessage(
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("opName")] string? OperatorName,
    [property: JsonPropertyName("full")] int? EmptyToPettyCash);

internal sealed record StackerCancellationRequestMessage(
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("peripheral")] int Peripheral,
    [property: JsonPropertyName("opName")] string? OperatorName);

internal sealed record EmptyingStatusRequestMessage(
    [property: JsonPropertyName("tipo")] VneRequestType RequestType);

internal sealed record CoinDispenserEmptyingRequestMessage(
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("opName")] string? OperatorName,
    [property: JsonPropertyName("num")] int HopperNumber);
