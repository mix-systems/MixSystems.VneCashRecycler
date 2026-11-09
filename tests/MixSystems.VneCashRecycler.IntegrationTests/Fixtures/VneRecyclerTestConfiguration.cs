using System.Text.Json;

namespace MixSystems.VneCashRecycler.IntegrationTests.Fixtures;

internal sealed class VneRecyclerTestConfiguration
{
    public bool UseEmbeddedSimulator { get; set; } = true;

    public string? Endpoint { get; set; }

    public bool SkipCertificateValidation { get; set; } = true;

    public int AttemptTimeoutSeconds { get; set; } = 5;

    public int RetryCount { get; set; } = 1;

    public double RetryDelaySeconds { get; set; } = 0;

    public string? DefaultOperatorName { get; set; }

    public static VneRecyclerTestConfiguration Load(string fileName)
    {
        string path = Path.Combine(AppContext.BaseDirectory, fileName);
        if (!File.Exists(path))
        {
            return new VneRecyclerTestConfiguration();
        }

        using FileStream stream = File.OpenRead(path);
        VneRecyclerTestConfiguration? settings = JsonSerializer.Deserialize<VneRecyclerTestConfiguration>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return settings ?? new VneRecyclerTestConfiguration();
    }
}
