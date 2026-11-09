using MixSystems.VneCashRecycler.ValueObjects;

namespace MixSystems.VneCashRecycler.Models;

public sealed record PaymentProgress(
    PaymentId PaymentId,
    PaymentLifecycleStatus LifecycleStatus,
    PaymentDetailStatus DetailStatus,
    MoneyCents Amount,
    MoneyCents Inserted,
    MoneyCents Rest,
    bool NotReturned,
    string? CancelledBy,
    PaymentTenderType Tender);
