namespace MixSystems.VneCashRecycler.Models;

/// <summary>
/// Command types supported by the VNE Automatic Cash protocol (see Appendix).
/// </summary>
public enum VneRequestType
{
    PaymentRequest = 1,
    PaymentStatusPolling = 2,
    DeletePendingPayment = 3,
    PendingPaymentsList = 5,
    OperatorWithdrawalRequest = 10,
    OperatorWithdrawalPolling = 11,
    EndOperatorWithdrawal = 12,
    MachineStatus = 20,
    StartRefill = 30,
    EndRefill = 31,
    ManualRefill = 32,
    RefillPolling = 33,
    OperatorLogin = 40,
    OperatorLogout = 41,
    HopperEmptying = 50,
    RecyclerEmptying = 51,
    StackerCancellation = 52,
    EmptyingPolling = 53,
    CoinDispenserEmptying = 54,
    DoorOpening = 55,
    CashClosing = 60,
    Refund = 65,
    RefundPolling = 66,
    WithdrawalsList = 70,
    PaymentsList = 71,
    OpeningsList = 72,
    CashClosingList = 73,
    AccessConfigurationMenu = 80,
    RebootOrShutdown = 81,
    MachineVersion = 82,
    PeripheralsConfiguration = 90,
    SoftwareConfiguration = 91,
    PettyCashSetting = 92,
    EndPettyCashRestore = 93
}
