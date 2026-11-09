using MixSystems.VneCashRecycler.ValueObjects;

namespace MixSystems.VneCashRecycler.Models;

public sealed record WithdrawalEntry(DateTimeOffset Date, MoneyCents Amount, string OperatorName);

public sealed record PaymentEntry(
    DateTimeOffset Date,
    PaymentId PaymentId,
    string Status,
    MoneyCents Amount,
    MoneyCents Inserted,
    MoneyCents Rest,
    MoneyCents Refund,
    string OperatorName,
    string? CancelledBy,
    PaymentTenderType Tender);

public enum OpeningEventType
{
    PhysicalOpen = 0,
    PhysicalClose = 1,
    Authorization = 2
}

public sealed record OpeningEntry(DateTimeOffset Date, OpeningEventType Type, string OperatorName);

public sealed record CashClosingEntry(
    string OperatorName,
    DateTimeOffset Date,
    DateTimeOffset StartDate,
    MoneyCents TotalIn,
    MoneyCents TotalOut,
    MoneyCents TotalPayments,
    MoneyCents TotalOperatorIn,
    MoneyCents TotalOperatorOut,
    MoneyCents TotalContent,
    MoneyCents? PettyCash,
    bool PettyCashRestored,
    MoneyCents CashIncome);

public sealed record WithdrawalReport(IReadOnlyList<WithdrawalEntry> Items);
public sealed record PaymentReport(IReadOnlyList<PaymentEntry> Items);
public sealed record OpeningReport(IReadOnlyList<OpeningEntry> Items);
public sealed record CashClosingHistoryReport(IReadOnlyList<CashClosingEntry> Items);
