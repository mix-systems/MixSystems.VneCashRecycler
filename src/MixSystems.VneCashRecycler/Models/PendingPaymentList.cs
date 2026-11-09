using MixSystems.VneCashRecycler.ValueObjects;

namespace MixSystems.VneCashRecycler.Models;

public sealed record PendingPaymentList(IReadOnlyList<PaymentId> PaymentIds);
