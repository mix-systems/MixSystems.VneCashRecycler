using System.Text.Json.Serialization;
using MixSystems.VneCashRecycler.Models;

namespace MixSystems.VneCashRecycler.Protocol.Responses;

internal sealed record CashClosingResponse(
    [property: JsonPropertyName("req_status")] int? RequestStatus,
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("date")] long? Date,
    [property: JsonPropertyName("start_date")] long? StartDate,
    [property: JsonPropertyName("total_in")] int? TotalIn,
    [property: JsonPropertyName("total_out")] int? TotalOut,
    [property: JsonPropertyName("total_payments")] int? TotalPayments,
    [property: JsonPropertyName("total_operator_in")] int? TotalOperatorIn,
    [property: JsonPropertyName("total_operator_out")] int? TotalOperatorOut,
    [property: JsonPropertyName("total_content")] int? TotalContent,
    [property: JsonPropertyName("petty_cash")] string? PettyCash,
    [property: JsonPropertyName("cash_income")] int? CashIncome,
    [property: JsonPropertyName("mess")] int? ErrorCode,
    [property: JsonPropertyName("operation")] int? Operation,
    [property: JsonPropertyName("user")] string? OperationUser);

internal sealed record TimelineResponse(
    [property: JsonPropertyName("req_status")] int? RequestStatus,
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("timeline")] TimelineEntryPayload[]? Entries,
    [property: JsonPropertyName("mess")] int? ErrorCode,
    [property: JsonPropertyName("operation")] int? Operation,
    [property: JsonPropertyName("user")] string? OperationUser);

internal sealed record TimelineEntryPayload(
    [property: JsonPropertyName("date")] long Timestamp,
    [property: JsonPropertyName("event")] string Event,
    [property: JsonPropertyName("operator")] string Operator);

internal sealed record PaymentListResponse(
    [property: JsonPropertyName("req_status")] int? RequestStatus,
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("payments")] PaymentEntryPayload[]? Payments,
    [property: JsonPropertyName("mess")] int? ErrorCode,
    [property: JsonPropertyName("operation")] int? Operation,
    [property: JsonPropertyName("user")] string? OperationUser);

internal sealed record PaymentEntryPayload(
    [property: JsonPropertyName("date")] long Date,
    [property: JsonPropertyName("id")] string PaymentId,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("amount")] int Amount,
    [property: JsonPropertyName("inserted")] int Inserted,
    [property: JsonPropertyName("rest")] int Rest,
    [property: JsonPropertyName("refund")] int Refund,
    [property: JsonPropertyName("operator")] string Operator,
    [property: JsonPropertyName("cancelled_by")] string? CancelledBy);

internal sealed record WithdrawalListResponse(
    [property: JsonPropertyName("req_status")] int? RequestStatus,
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("withdrawals")] WithdrawalEntryPayload[]? Withdrawals,
    [property: JsonPropertyName("mess")] int? ErrorCode,
    [property: JsonPropertyName("operation")] int? Operation,
    [property: JsonPropertyName("user")] string? OperationUser);

internal sealed record WithdrawalEntryPayload(
    [property: JsonPropertyName("date")] long Date,
    [property: JsonPropertyName("value")] int Value,
    [property: JsonPropertyName("operator")] string Operator);

internal sealed record OpeningListResponse(
    [property: JsonPropertyName("req_status")] int? RequestStatus,
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("openings")] OpeningEntryPayload[]? Openings,
    [property: JsonPropertyName("mess")] int? ErrorCode,
    [property: JsonPropertyName("operation")] int? Operation,
    [property: JsonPropertyName("user")] string? OperationUser);

internal sealed record OpeningEntryPayload(
    [property: JsonPropertyName("date")] long Date,
    [property: JsonPropertyName("type")] int Type,
    [property: JsonPropertyName("operator")] string Operator);

internal sealed record CashClosingHistoryResponse(
    [property: JsonPropertyName("req_status")] int? RequestStatus,
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("closings")] CashClosingEntryPayload[]? Closings,
    [property: JsonPropertyName("mess")] int? ErrorCode,
    [property: JsonPropertyName("operation")] int? Operation,
    [property: JsonPropertyName("user")] string? OperationUser);

internal sealed record CashClosingEntryPayload(
    [property: JsonPropertyName("operator")] string Operator,
    [property: JsonPropertyName("date")] long Date,
    [property: JsonPropertyName("start_date")] long StartDate,
    [property: JsonPropertyName("total_in")] int TotalIn,
    [property: JsonPropertyName("total_out")] int TotalOut,
    [property: JsonPropertyName("total_payments")] int TotalPayments,
    [property: JsonPropertyName("total_operator_in")] int TotalOperatorIn,
    [property: JsonPropertyName("total_operator_out")] int TotalOperatorOut,
    [property: JsonPropertyName("total_content")] int TotalContent,
    [property: JsonPropertyName("petty_cash")] string PettyCash,
    [property: JsonPropertyName("cash_income")] int CashIncome);
