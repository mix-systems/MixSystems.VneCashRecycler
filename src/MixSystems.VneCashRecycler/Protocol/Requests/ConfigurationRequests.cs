using System.Text.Json.Serialization;
using MixSystems.VneCashRecycler.Models;

namespace MixSystems.VneCashRecycler.Protocol.Requests;

internal sealed record PeripheralsConfigurationRequestMessage(
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("opName")] string? OperatorName,
    [property: JsonPropertyName("config")] IReadOnlyDictionary<string, int> ConfigurationValues);

internal sealed record SoftwareConfigurationRequestMessage(
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("opName")] string? OperatorName,
    [property: JsonPropertyName("config")] IReadOnlyDictionary<string, int> ConfigurationValues);

internal sealed record PettyCashSettingRequestMessage(
    [property: JsonPropertyName("tipo")] VneRequestType RequestType,
    [property: JsonPropertyName("opName")] string? OperatorName,
    [property: JsonPropertyName("pettycash")] long PettyCashValue);

internal sealed record EndPettyCashRestoreRequestMessage(
    [property: JsonPropertyName("tipo")] VneRequestType RequestType);
