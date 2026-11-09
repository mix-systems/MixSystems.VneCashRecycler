namespace MixSystems.VneCashRecycler.Configuration;

/// <summary>
/// Configures retry/timeout behavior when communicating with the recycler.
/// </summary>
public sealed class VneResilienceOptions
{
    /// <summary>
    /// Timeout for a single HTTP attempt in seconds (default 5 seconds from protocol doc).
    /// </summary>
    public int AttemptTimeoutSeconds { get; set; } = 5;

    /// <summary>
    /// Number of retry attempts automatically executed after a timeout (default 1 to satisfy protocol requirement).
    /// </summary>
    public int RetryCount { get; set; } = 1;

    /// <summary>
    /// Base delay (seconds) between retry attempts. Set to 0 to retry immediately.
    /// </summary>
    public double RetryDelaySeconds { get; set; } = 0;

    /// <summary>
    /// Failures threshold before opening a circuit breaker (0 disables the breaker).
    /// </summary>
    public int CircuitBreakerFailures { get; set; } = 5;

    /// <summary>
    /// Duration (seconds) to keep the circuit breaker open.
    /// </summary>
    public int CircuitBreakerDurationSeconds { get; set; } = 5;
}
