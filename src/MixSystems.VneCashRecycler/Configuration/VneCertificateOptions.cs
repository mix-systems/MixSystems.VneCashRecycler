namespace MixSystems.VneCashRecycler.Configuration;

/// <summary>
/// Certificate validation configuration for HTTPS communication with the recycler.
/// </summary>
public sealed class VneCertificateOptions
{
    /// <summary>
    /// When true, the SDK will trust the recycler certificate without validation (dev only).
    /// </summary>
    public bool SkipCertificateValidation { get; set; } = true;

    /// <summary>
    /// Optional thumbprint that must match the server certificate.
    /// </summary>
    public string? TrustedCertificateThumbprint { get; set; }
        = null;
}
