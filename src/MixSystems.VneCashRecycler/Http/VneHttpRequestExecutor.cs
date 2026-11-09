using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MixSystems.VneCashRecycler.Exceptions;
using MixSystems.VneCashRecycler.Serialization;

namespace MixSystems.VneCashRecycler.Http;

internal sealed class VneHttpRequestExecutor
{
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly ILogger<VneHttpRequestExecutor> _logger;

    /// <summary>
    /// Creates the executor responsible for serializing protocol payloads and logging transport operations.
    /// </summary>
    public VneHttpRequestExecutor(VneJsonSerializerOptionsAccessor optionsAccessor, ILogger<VneHttpRequestExecutor> logger)
    {
        _serializerOptions = optionsAccessor.Options;
        _logger = logger;
    }

    /// <summary>
    /// Sends a POST request to the recycler endpoint and deserializes the JSON response.
    /// </summary>
    public async Task<TResponse> PostAsync<TRequest, TResponse>(HttpClient httpClient, Uri endpoint, TRequest payload, CancellationToken cancellationToken)
    {
        string json = JsonSerializer.Serialize(payload, _serializerOptions);

        using HttpRequestMessage message = new(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        _logger.LogDebug("Sending request to {Endpoint}: {Payload}", endpoint, json);

        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new VneTransportException("Failed to send request to recycler.", ex);
        }

        using (response)
        {
            string body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogDebug("Received response ({StatusCode}): {Body}", response.StatusCode, body);

            if (!response.IsSuccessStatusCode)
            {
                throw new VneTransportException($"Recycler returned HTTP {(int)response.StatusCode}: {body}");
            }

            try
            {
                return JsonSerializer.Deserialize<TResponse>(body, _serializerOptions)
                    ?? throw new VneTransportException("Recycler response could not be deserialized.");
            }
            catch (JsonException jsonException)
            {
                throw new VneTransportException("Invalid JSON payload received from recycler.", jsonException);
            }
        }
    }
}
