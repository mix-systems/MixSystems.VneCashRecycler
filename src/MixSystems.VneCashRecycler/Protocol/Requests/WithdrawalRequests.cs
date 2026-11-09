using System.Text.Json.Serialization;
using MixSystems.VneCashRecycler.Models;

namespace MixSystems.VneCashRecycler.Protocol.Requests;

internal sealed record WithdrawalRequestMessage(
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("importo")] long Amount,
    [property: JsonPropertyName("opName")] string? OperatorName,
    [property: JsonPropertyName("taglio")] string? Denomination,
    [property: JsonPropertyName("operatore")] string? OperatorIdentifier);

internal sealed record WithdrawalStatusRequestMessage(
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("id")] string WithdrawalId,
    [property: JsonPropertyName("opName")] string? OperatorName);

internal sealed record EndWithdrawalRequestMessage(
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("opName")] string? OperatorName);
