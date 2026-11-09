using System.Text;
using MixSystems.VneCashRecycler.Models;
using MixSystems.VneCashRecycler.Protocol;

namespace MixSystems.VneCashRecycler.Exceptions;

/// <summary>
/// Represents a NACK response (req_status = 2) from the recycler.
/// </summary>
public sealed class VneApiException : VneException
{
    /// <summary>
    /// Creates an exception representing a VNE NACK response for the specified command type.
    /// </summary>
    public VneApiException(VneRequestType requestType, int? errorCode, string? rawMessage = null, int? operation = null, string? operationUser = null)
        : this(requestType, errorCode, rawMessage, operation, operationUser, VneErrorCatalog.GetDescription(errorCode))
    {
    }

    private VneApiException(VneRequestType requestType, int? errorCode, string? rawMessage, int? operation, string? operationUser, string? description)
        : base(BuildMessage(requestType, errorCode, rawMessage, operation, operationUser, description))
    {
        RequestType = requestType;
        ErrorCode = errorCode;
        RawMessage = rawMessage;
        Operation = operation;
        OperationUser = operationUser;
        ErrorDescription = description;
    }

    public VneRequestType RequestType { get; }

    public int? ErrorCode { get; }

    public string? RawMessage { get; }

    public string? ErrorDescription { get; }

    public int? Operation { get; }

    public string? OperationUser { get; }

    private static string BuildMessage(VneRequestType requestType, int? errorCode, string? rawMessage, int? operation, string? operationUser, string? description)
    {
        StringBuilder builder = new();
        builder.Append($"Recycler rejected request type {(int)requestType} ({requestType})");

        if (errorCode.HasValue)
        {
            builder.Append($" with error code {errorCode.Value}");
            if (!string.IsNullOrWhiteSpace(description))
            {
                builder.Append($" ({description})");
            }
        }
        else
        {
            builder.Append(" with unknown error code");
        }

        if (!string.IsNullOrWhiteSpace(rawMessage))
        {
            builder.Append($". Message: {rawMessage}");
        }

        if (operation.HasValue)
        {
            builder.Append($". Operation in progress: {operation}");
            if (!string.IsNullOrWhiteSpace(operationUser))
            {
                builder.Append($" (user: {operationUser})");
            }
        }

        builder.Append('.');
        return builder.ToString();
    }
}
