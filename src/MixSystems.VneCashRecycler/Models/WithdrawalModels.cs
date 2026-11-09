using MixSystems.VneCashRecycler.ValueObjects;

namespace MixSystems.VneCashRecycler.Models;

public enum WithdrawalStatus
{
    Completed = 1,
    NotCompleted = 2
}

public sealed record WithdrawalRequestResult(WithdrawalId WithdrawalId, MoneyCents Amount);

public sealed record WithdrawalProgress(
    WithdrawalId WithdrawalId,
    WithdrawalStatus Status,
    MoneyCents RequestedAmount,
    MoneyCents WithdrawnAmount);

public readonly record struct WithdrawalDenomination
{
    public string Value { get; }

    private WithdrawalDenomination(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public static WithdrawalDenomination MixedCoins => new("monete");
    public static WithdrawalDenomination All => new("all");

    public static WithdrawalDenomination Coins(int cents)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(cents, 1);
        return new($"monete_{cents}");
    }

    public static WithdrawalDenomination Banknote(int cents)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(cents, 1);
        return new($"banconota_{cents}");
    }

    public override string ToString() => Value;
}
