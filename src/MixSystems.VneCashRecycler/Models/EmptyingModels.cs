namespace MixSystems.VneCashRecycler.Models;

public enum StackerPeripheral
{
    CoinsCashbox = 0,
    BanknotesStacker = 1,
    Both = 2
}

public enum CoinDispenserSelection
{
    Hopper1 = 1,
    Hopper2 = 2
}

public sealed record EmptyingStatusResult(bool EmptyingInProgress);
