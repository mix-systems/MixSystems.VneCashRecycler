using MixSystems.VneCashRecycler.ValueObjects;

namespace MixSystems.VneCashRecycler.Models;

public sealed record CashClosingReport(
    DateTimeOffset Date,
    DateTimeOffset StartDate,
    MoneyCents TotalInserted,
    MoneyCents TotalDispensed,
    MoneyCents TotalPayments,
    MoneyCents TotalOperatorIn,
    MoneyCents TotalOperatorOut,
    MoneyCents TotalContent,
    MoneyCents? PettyCash,
    bool PettyCashRestored,
    MoneyCents CashIncome);

public enum RefundStatus
{
    Pending,
    Completed
}

public sealed record RefundProgress(PaymentId PaymentId, RefundStatus Status, MoneyCents Refunded, MoneyCents ToRefund);
