namespace MixSystems.VneCashRecycler.Models;

public sealed record CassetteStatus(int Denomination, int Quantity, int Alert, int MinLevel, int MaxLevel);

public sealed record CoinStatus(int Denomination, int Quantity, int Alert, int MinLevel, int MaxLevel, bool Accepts, bool Recycles, int CashboxCount);

public sealed record RecyclerStatus(
    int Error,
    string Message,
    IReadOnlyList<CassetteStatus> Cassettes,
    int? Stacker,
    int? TotalInRecycle,
    IReadOnlyDictionary<int, int>? StackerDetail);

public sealed record HopperStatus(
    int Error,
    string Message,
    IReadOnlyList<CoinStatus> Coins,
    int? Stacker,
    int? TotalInRecycle);

public sealed record DispenserCassetteStatus(int Denomination, int Quantity, int Alert, int Error);

public sealed record DispenserStatus(
    int Error,
    string Message,
    IReadOnlyList<DispenserCassetteStatus> Cassettes,
    int? TotalInRecycle,
    int? RejectBin);

public sealed record CoinDispenserHopperStatus(int Index, int Denomination, int Quantity, int Alert, int Error);

public sealed record CoinDispenserStatus(IReadOnlyList<CoinDispenserHopperStatus> Hoppers);

public sealed record MachineStatus(
    int RequestStatus,
    RecyclerStatus? Recycler,
    HopperStatus? Hopper,
    DispenserStatus? Dispenser,
    CoinDispenserStatus? CoinDispenser);
