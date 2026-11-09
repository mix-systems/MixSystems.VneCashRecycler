using System.Text.Json.Serialization;
using MixSystems.VneCashRecycler.Models;

namespace MixSystems.VneCashRecycler.Protocol.Responses;

internal sealed record PaymentCancellationResponse(
    [property: JsonPropertyName("req_status")] int? RequestStatus,
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("payment_status")] int? PaymentStatus,
    [property: JsonPropertyName("mess")] int? ErrorCode,
    [property: JsonPropertyName("id")] string? PaymentId,
    [property: JsonPropertyName("operation")] int? Operation,
    [property: JsonPropertyName("user")] string? OperationUser);

internal sealed record PaymentStatusResponse(
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("id")] string PaymentId,
    [property: JsonPropertyName("req_status")] int? RequestStatus,
    [property: JsonPropertyName("payment_status")] int PaymentStatus,
    [property: JsonPropertyName("payment_details")] PaymentDetails Details,
    [property: JsonPropertyName("mess")] int? ErrorCode,
    [property: JsonPropertyName("operation")] int? Operation,
    [property: JsonPropertyName("user")] string? OperationUser);

internal sealed record PaymentDetails(
    [property: JsonPropertyName("amount")] int Amount,
    [property: JsonPropertyName("inserted")] int Inserted,
    [property: JsonPropertyName("rest")] int Rest,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("not_returned")] int? NotReturned,
    [property: JsonPropertyName("cancelled_by")] string? CancelledBy);

internal sealed record PendingPaymentsResponse(
    [property: JsonPropertyName("req_status")] int? RequestStatus,
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("list_length"), JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] int Length,
    [property: JsonPropertyName("pending_list")] Dictionary<string, string>? PendingList,
    [property: JsonPropertyName("mess")] int? ErrorCode,
    [property: JsonPropertyName("operation")] int? Operation,
    [property: JsonPropertyName("user")] string? OperationUser);

internal sealed record RefundStatusResponse(
    [property: JsonPropertyName("req_status")] int? RequestStatus,
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("id")] string PaymentId,
    [property: JsonPropertyName("refund_status")] string? RefundStatus,
    [property: JsonPropertyName("refunded")] int? Refunded,
    [property: JsonPropertyName("toRefund")] int? ToRefund,
    [property: JsonPropertyName("mess")] int? ErrorCode,
    [property: JsonPropertyName("operation")] int? Operation,
    [property: JsonPropertyName("user")] string? OperationUser);
