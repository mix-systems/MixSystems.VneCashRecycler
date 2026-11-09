using System.Text.Json.Serialization;
using MixSystems.VneCashRecycler.Models;

namespace MixSystems.VneCashRecycler.Protocol.Responses;

internal sealed record AcknowledgementResponse(
    [property: JsonPropertyName("req_status")] int? RequestStatus,
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("mess")] int? ErrorCode,
    [property: JsonPropertyName("importo")] int? Amount,
    [property: JsonPropertyName("id")] string? PaymentId,
    [property: JsonPropertyName("operation")] int? Operation,
    [property: JsonPropertyName("user")] string? OperationUser);
