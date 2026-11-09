using System.Text.Json.Serialization;
using MixSystems.VneCashRecycler.Models;

namespace MixSystems.VneCashRecycler.Protocol.Requests;

internal sealed record PaymentRequestMessage(
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("importo")] long Amount,
    [property: JsonPropertyName("opName")] string? OperatorName,
    [property: JsonPropertyName("operatore")] string? OperatorIdentifier,
    [property: JsonPropertyName("refundable")] int? Refundable,
    [property: JsonPropertyName("credit_card")] int? CreditCard,
    [property: JsonPropertyName("authKey")] string? AuthenticationKey);

internal sealed record PaymentStatusRequestMessage(
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("id")] string PaymentId,
    [property: JsonPropertyName("opName")] string? OperatorName);

internal sealed record DeletePaymentRequestMessage(
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("id")] string PaymentId,
    [property: JsonPropertyName("tipo_annullamento")] int CancellationMode,
    [property: JsonPropertyName("opName")] string? OperatorName);

internal sealed record PendingPaymentsRequestMessage(
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("opName")] string? OperatorName);

internal sealed record RefundRequestMessage(
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("id")] string PaymentId,
    [property: JsonPropertyName("opName")] string? OperatorName);

internal sealed record RefundStatusRequestMessage(
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("id")] string PaymentId);
