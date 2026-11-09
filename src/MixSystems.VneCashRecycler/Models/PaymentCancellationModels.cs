using MixSystems.VneCashRecycler.ValueObjects;

namespace MixSystems.VneCashRecycler.Models;

/// <summary>
/// Result codes returned by the VNE API when cancelling a pending payment.
/// </summary>
public enum PaymentCancellationStatus
{
    /// <summary>
    /// Payment deleted before being shown on screen (payment_status = 1).
    /// </summary>
    DeletedBeforeProcessing = 1,

    /// <summary>
    /// Cancellation queued while the machine is still processing the payment (payment_status = 2).
    /// </summary>
    Pending = 2,

    /// <summary>
    /// Payment already completed or deleted, therefore cannot be cancelled (payment_status = 3).
    /// </summary>
    NotCancellableAlreadyProcessed = 3,

    /// <summary>
    /// Machine cannot return the amount, only partial acceptance is possible (payment_status = 4).
    /// </summary>
    ReturnNotAvailable = 4
}

public sealed record PaymentCancellationResult(PaymentId PaymentId, PaymentCancellationStatus Status);
