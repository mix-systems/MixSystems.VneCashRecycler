namespace MixSystems.VneCashRecycler.Exceptions;

/// <summary>
/// Represents connectivity issues (timeouts, TLS failures, HTTP errors).
/// </summary>
public sealed class VneTransportException : VneException
{
    /// <summary>
    /// Initializes a transport exception with the provided message and optional inner exception.
    /// </summary>
    public VneTransportException(string message, Exception? inner = null)
        : base(message, inner ?? new Exception(message))
    {
    }
}
