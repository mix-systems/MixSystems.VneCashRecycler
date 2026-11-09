using System.Text.Json.Serialization;
using MixSystems.VneCashRecycler.Models;

namespace MixSystems.VneCashRecycler.Protocol.Responses;

internal sealed record RefillStatusResponse(
    [property: JsonPropertyName("req_status")] int? RequestStatus,
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("refillOn")] int? RefillOn,
    [property: JsonPropertyName("amountRefill")] int? AmountRefill,
    [property: JsonPropertyName("mess")] int? ErrorCode,
    [property: JsonPropertyName("operation")] int? Operation,
    [property: JsonPropertyName("user")] string? OperationUser);

internal sealed record EmptyingStatusResponse(
    [property: JsonPropertyName("req_status")] int? RequestStatus,
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("empty_status")] int? EmptyStatus,
    [property: JsonPropertyName("mess")] int? ErrorCode,
    [property: JsonPropertyName("operation")] int? Operation,
    [property: JsonPropertyName("user")] string? OperationUser);
