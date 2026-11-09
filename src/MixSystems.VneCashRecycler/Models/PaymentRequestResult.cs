using MixSystems.VneCashRecycler.ValueObjects;

namespace MixSystems.VneCashRecycler.Models;

public sealed record PaymentRequestResult(PaymentId PaymentId, MoneyCents Amount);
