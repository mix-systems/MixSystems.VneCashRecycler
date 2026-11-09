namespace MixSystems.VneCashRecycler.Models;

public enum PaymentLifecycleStatus
{
    Completed = 1,
    Pending = 2
}

public enum PaymentDetailStatus
{
    Completed,
    Returned,
    Partial,
    Deleted,
    Pending,
    NotCompleted
}

public enum PaymentTenderType
{
    Cash = 0,
    Card = 1
}
