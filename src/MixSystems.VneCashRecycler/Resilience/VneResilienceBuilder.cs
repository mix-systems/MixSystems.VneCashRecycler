using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Timeout;
using MixSystems.VneCashRecycler.Configuration;

namespace MixSystems.VneCashRecycler.Resilience;

internal static class VneResilienceBuilder
{
    /// <summary>
    /// Builds the HTTP resilience policy (timeout + optional circuit breaker) dictated by the protocol.
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> Build(VneResilienceOptions options)
    {
        TimeSpan attemptTimeout = TimeSpan.FromSeconds(Math.Max(1, options.AttemptTimeoutSeconds));
        IAsyncPolicy<HttpResponseMessage> pipeline = Policy.TimeoutAsync<HttpResponseMessage>(attemptTimeout);

        if (options.RetryCount > 0)
        {
            IAsyncPolicy<HttpResponseMessage> retryPolicy = Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .Or<TaskCanceledException>()
                .Or<TimeoutRejectedException>()
                .OrResult(response => (int)response.StatusCode >= 500)
                .WaitAndRetryAsync(
                    options.RetryCount,
                    attempt => CalculateRetryDelay(options, attempt));

            pipeline = Policy.WrapAsync(retryPolicy, pipeline);
        }

        if (options.CircuitBreakerFailures > 0)
        {
            AsyncCircuitBreakerPolicy<HttpResponseMessage>? breaker = HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    options.CircuitBreakerFailures,
                    TimeSpan.FromSeconds(Math.Max(1, options.CircuitBreakerDurationSeconds)));

            pipeline = Policy.WrapAsync(breaker, pipeline);
        }

        return pipeline;
    }

    private static TimeSpan CalculateRetryDelay(VneResilienceOptions options, int attempt)
    {
        double baseDelay = Math.Max(0, options.RetryDelaySeconds);
        if (baseDelay <= 0)
        {
            return TimeSpan.Zero;
        }

        double exponential = baseDelay * Math.Pow(2, Math.Max(0, attempt - 1));
        double jitterFactor = 0.75 + (Random.Shared.NextDouble() * 0.5); // jitter in [0.75, 1.25]
        double capped = Math.Min(exponential * jitterFactor, options.AttemptTimeoutSeconds);
        return TimeSpan.FromSeconds(capped);
    }
}
