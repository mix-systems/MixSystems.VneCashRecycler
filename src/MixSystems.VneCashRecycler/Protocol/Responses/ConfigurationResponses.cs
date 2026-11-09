using System.Text.Json.Serialization;
using MixSystems.VneCashRecycler.Models;

namespace MixSystems.VneCashRecycler.Protocol.Responses;

internal sealed record MenuAccessResponse(
    [property: JsonPropertyName("req_status")] int? RequestStatus,
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("mess")] int? ErrorCode,
    [property: JsonPropertyName("operation")] int? Operation,
    [property: JsonPropertyName("user")] string? OperationUser);

internal sealed record RebootResponse(
    [property: JsonPropertyName("req_status")] int? RequestStatus,
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("mess")] int? ErrorCode,
    [property: JsonPropertyName("operation")] int? Operation,
    [property: JsonPropertyName("user")] string? OperationUser);

internal sealed record VersionResponse(
    [property: JsonPropertyName("req_status")] int? RequestStatus,
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("version")] string? Version,
    [property: JsonPropertyName("mess")] int? ErrorCode,
    [property: JsonPropertyName("operation")] int? Operation,
    [property: JsonPropertyName("user")] string? OperationUser);

internal sealed record PeripheralsConfigurationResponse(
    [property: JsonPropertyName("req_status")] int? RequestStatus,
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("mess")] int? ErrorCode,
    [property: JsonPropertyName("operation")] int? Operation,
    [property: JsonPropertyName("user")] string? OperationUser);

internal sealed record SoftwareConfigurationResponse(
    [property: JsonPropertyName("req_status")] int? RequestStatus,
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("mess")] int? ErrorCode,
    [property: JsonPropertyName("operation")] int? Operation,
    [property: JsonPropertyName("user")] string? OperationUser);

internal sealed record PettyCashSettingResponse(
    [property: JsonPropertyName("req_status")] int? RequestStatus,
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("mess")] int? ErrorCode,
    [property: JsonPropertyName("operation")] int? Operation,
    [property: JsonPropertyName("user")] string? OperationUser);
