using System.Text.Json.Serialization;
using MixSystems.VneCashRecycler.Models;

namespace MixSystems.VneCashRecycler.Protocol.Requests;

internal sealed record MachineStatusRequestMessage(
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("opName")] string? OperatorName);

internal sealed record OperatorLoginRequestMessage(
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("opName")] string? OperatorName);

internal sealed record OperatorLogoutRequestMessage(
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("opName")] string? OperatorName);

internal sealed record DoorOpeningRequestMessage(
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("wait_timeout")] string? WaitTimeout,
    [property: JsonPropertyName("opName")] string? OperatorName,
    [property: JsonPropertyName("port")] int? Port);

internal sealed record CashClosingRequestMessage(
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("opName")] string? OperatorName);

internal sealed record TimelineRequestMessage(
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("start_date")] long? StartDate,
    [property: JsonPropertyName("end_date")] long? EndDate,
    [property: JsonPropertyName("opName")] string? OperatorName);

internal sealed record MenuAccessRequestMessage(
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("userLevel")] int UserLevel,
    [property: JsonPropertyName("opName")] string? OperatorName,
    [property: JsonPropertyName("password")] string? Password);

internal sealed record RebootRequestMessage(
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("opName")] string? OperatorName,
    [property: JsonPropertyName("restart")] int RestartFlag);

internal sealed record VersionRequestMessage(
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("opName")] string? OperatorName);
