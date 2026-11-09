using System.Text.Json.Serialization;
using MixSystems.VneCashRecycler.Models;

namespace MixSystems.VneCashRecycler.Protocol.Responses;

internal sealed record WithdrawalStatusResponse(
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("id")] string WithdrawalId,
    [property: JsonPropertyName("req_status")] int? RequestStatus,
    [property: JsonPropertyName("withdraw_status")] int Status,
    [property: JsonPropertyName("payment_details")] WithdrawalDetails? Details,
    [property: JsonPropertyName("mess")] int? ErrorCode,
    [property: JsonPropertyName("operation")] int? Operation,
    [property: JsonPropertyName("user")] string? OperationUser);

internal sealed record WithdrawalDetails(
    [property: JsonPropertyName("amount")] int Amount,
    [property: JsonPropertyName("withdrawed")] int Withdrawn,
    [property: JsonPropertyName("operation")] int? Operation,
    [property: JsonPropertyName("user")] string? OperationUser);
