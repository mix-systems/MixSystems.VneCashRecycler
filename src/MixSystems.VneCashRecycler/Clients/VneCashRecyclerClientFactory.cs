using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using Polly;
using MixSystems.VneCashRecycler.Configuration;
using MixSystems.VneCashRecycler.Http;
using MixSystems.VneCashRecycler.Resilience;
using static System.String;

namespace MixSystems.VneCashRecycler.Clients;

internal sealed class VneCashRecyclerClientFactory(VneHttpRequestExecutor executor, ILoggerFactory loggerFactory)
    : IVneCashRecyclerClientFactory
{
    /// <inheritdoc />
    public IVneCashRecyclerClient Create(VneCashRecyclerClientOptions options)
    {
        HttpMessageHandler handler = CreateHandler(options.Certificate);
        IAsyncPolicy<HttpResponseMessage> policy = VneResilienceBuilder.Build(options.Resilience);
        PolicyDelegatingHandler policyHandler = new(policy);
        policyHandler.InnerHandler = handler;

        HttpClient httpClient = new(policyHandler)
        {
            Timeout = Timeout.InfiniteTimeSpan,
            DefaultRequestVersion = HttpVersion.Version11,
            DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower
        };

        ILogger<VneCashRecyclerClient> logger = loggerFactory.CreateLogger<VneCashRecyclerClient>();

        return new VneCashRecyclerClient(httpClient, options, executor, logger);
    }

    private static HttpMessageHandler CreateHandler(VneCertificateOptions certificateOptions)
    {
        SocketsHttpHandler handler = new SocketsHttpHandler
        {
            AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
            PooledConnectionLifetime = TimeSpan.FromMinutes(2),
            UseCookies = false
        };

        if (certificateOptions.SkipCertificateValidation)
        {
            handler.SslOptions = new SslClientAuthenticationOptions
            {
                RemoteCertificateValidationCallback = static (_, _, _, _) => true
            };
            return handler;
        }

        if (IsNullOrWhiteSpace(certificateOptions.TrustedCertificateThumbprint))
        {
            return handler;
        }

        string thumbprint = certificateOptions.TrustedCertificateThumbprint.Trim().Replace(" ", Empty)
            .ToUpperInvariant();
        handler.SslOptions = new SslClientAuthenticationOptions
        {
            RemoteCertificateValidationCallback = (_, cert, _, _) =>
            {
                if (cert is null)
                {
                    return false;
                }

                string? certThumbprint = cert switch
                {
                    X509Certificate2 cert2 => cert2.Thumbprint,
                    _ => cert.GetCertHashString()
                };

                return certThumbprint?.Replace(" ", Empty)
                    .Equals(thumbprint, StringComparison.OrdinalIgnoreCase) == true;
            }
        };


        return handler;
    }
}
