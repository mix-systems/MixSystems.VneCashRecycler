namespace MixSystems.VneCashRecycler.Exceptions;

/// <summary>
/// Base exception for all SDK-specific failures.
/// </summary>
public class VneException : Exception
{
    public VneException(string message)
        : base(message)
    {
    }

    public VneException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
