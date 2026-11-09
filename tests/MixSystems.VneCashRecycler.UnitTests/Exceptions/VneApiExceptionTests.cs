using MixSystems.VneCashRecycler.Exceptions;
using MixSystems.VneCashRecycler.Models;

namespace MixSystems.VneCashRecycler.UnitTests.Exceptions;

public class VneApiExceptionTests
{
    [Trait("Category", "Unit")]
    [Fact]
    public void Constructor_IncludesKnownErrorDescriptionAndOperationContext()
    {
        VneApiException exception = new(
            VneRequestType.PaymentRequest,
            110,
            "Busy",
            operation: 20,
            operationUser: "cashier-01");

        Assert.Equal(VneRequestType.PaymentRequest, exception.RequestType);
        Assert.Equal(110, exception.ErrorCode);
        Assert.Equal("Busy", exception.RawMessage);
        Assert.Equal(20, exception.Operation);
        Assert.Equal("cashier-01", exception.OperationUser);
        Assert.Contains("Cannot process new request: another operation is already in progress.", exception.Message);
        Assert.Contains("Operation in progress: 20 (user: cashier-01)", exception.Message);
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void Constructor_ReportsUnknownErrorCodeWhenMissing()
    {
        VneApiException exception = new(
            VneRequestType.MachineStatus,
            null);

        Assert.Null(exception.ErrorCode);
        Assert.Contains("unknown error code", exception.Message);
    }
}
