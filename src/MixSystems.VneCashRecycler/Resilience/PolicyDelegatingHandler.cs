using Polly;

namespace MixSystems.VneCashRecycler.Resilience;

internal sealed class PolicyDelegatingHandler : DelegatingHandler
{
    private readonly IAsyncPolicy<HttpResponseMessage> _policy;

    /// <summary>
    /// Wraps an <see cref="HttpMessageHandler"/> pipeline with the provided Polly policy.
    /// </summary>
    public PolicyDelegatingHandler(IAsyncPolicy<HttpResponseMessage> policy)
    {
        _policy = policy;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => _policy.ExecuteAsync((ct) => base.SendAsync(request, ct), cancellationToken);
}
