using MixSystems.VneCashRecycler.ValueObjects;

namespace MixSystems.VneCashRecycler.Models;

public sealed record MenuAccessRequest(int UserLevel, OperatorName? OperatorName, string? Password);

/// <summary>
/// Builder for the "Peripherals configuration" command (tipo = 90).
/// </summary>
public sealed class PeripheralsConfiguration
{
    private readonly Dictionary<string, int> _values = new();

    /// <summary>
    /// Sets the denomination (in cents) for a recycler cassette (command tipo = 90).
    /// </summary>
    public PeripheralsConfiguration SetRecyclerCassetteDenomination(int cassetteIndex, int cents)
    {
        _values[$"cassette_denom_{cassetteIndex}"] = cents;
        return this;
    }

    /// <summary>
    /// Sets the maximum inventory threshold for a recycler cassette (command tipo = 90).
    /// </summary>
    public PeripheralsConfiguration SetRecyclerMaxLevel(int cassetteIndex, int level)
    {
        _values[$"cassette_max_level_{cassetteIndex}"] = level;
        return this;
    }

    /// <summary>
    /// Sets the petty cash (minimum) threshold for a recycler cassette.
    /// </summary>
    public PeripheralsConfiguration SetRecyclerMinLevel(int cassetteIndex, int level)
    {
        _values[$"cassette_min_level_{cassetteIndex}"] = level;
        return this;
    }

    /// <summary>
    /// Sets the maximum number of coins for a hopper denomination.
    /// </summary>
    public PeripheralsConfiguration SetHopperMaxLevel(int cents, int level)
    {
        _values[$"hopper_max_level_{cents}"] = level;
        return this;
    }

    /// <summary>
    /// Sets the petty cash level for a hopper denomination.
    /// </summary>
    public PeripheralsConfiguration SetHopperMinLevel(int cents, int level)
    {
        _values[$"hopper_min_level_{cents}"] = level;
        return this;
    }

    /// <summary>
    /// Enables or disables acceptance of a hopper denomination during counting.
    /// </summary>
    public PeripheralsConfiguration SetHopperAccept(int cents, bool accept)
    {
        _values[$"hopper_accept_{cents}"] = accept ? 1 : 0;
        return this;
    }

    /// <summary>
    /// Enables or disables use of a hopper denomination for change.
    /// </summary>
    public PeripheralsConfiguration SetHopperRecycle(int cents, bool recycle)
    {
        _values[$"hopper_recycle_{cents}"] = recycle ? 1 : 0;
        return this;
    }

    /// <summary>
    /// Sets the denomination of a single coin-dispenser hopper.
    /// </summary>
    public PeripheralsConfiguration SetCoinDispenserDenomination(int hopperIndex, int cents)
    {
        _values[$"coinDisp_denom_{hopperIndex}"] = cents;
        return this;
    }

    /// <summary>
    /// Sets the maximum level of a coin-dispenser hopper.
    /// </summary>
    public PeripheralsConfiguration SetCoinDispenserMaxLevel(int hopperIndex, int level)
    {
        _values[$"coinDisp_max_level_{hopperIndex}"] = level;
        return this;
    }

    /// <summary>
    /// Sets the petty cash level of a coin-dispenser hopper.
    /// </summary>
    public PeripheralsConfiguration SetCoinDispenserMinLevel(int hopperIndex, int level)
    {
        _values[$"coinDisp_min_level_{hopperIndex}"] = level;
        return this;
    }

    /// <summary>
    /// Enables or disables a coin-dispenser hopper.
    /// </summary>
    public PeripheralsConfiguration SetCoinDispenserEnable(int hopperIndex, bool enabled)
    {
        _values[$"coinDisp_enable_{hopperIndex}"] = enabled ? 1 : 0;
        return this;
    }

    internal IReadOnlyDictionary<string, int> Values => _values;
}

/// <summary>
/// Builder for the "Software configuration" command (tipo = 91).
/// </summary>
public sealed class SoftwareConfiguration
{
    private readonly Dictionary<string, int> _values = new();

    /// <summary>
    /// Sets the maximum cash payment amount accepted by the machine (command tipo = 91).
    /// </summary>
    public SoftwareConfiguration SetCashMaxLimit(int cents)
    {
        _values["cash_max_limit"] = cents;
        return this;
    }

    /// <summary>
    /// Configures how the machine handles multiple concurrent payments (single_payment option).
    /// </summary>
    public SoftwareConfiguration SetSinglePaymentMode(int mode)
    {
        _values["single_payment"] = mode;
        return this;
    }

    /// <summary>
    /// Determines whether refunds are allowed and under which conditions.
    /// </summary>
    public SoftwareConfiguration SetAllowRefund(int mode)
    {
        _values["allow_refund"] = mode;
        return this;
    }

    /// <summary>
    /// Sets the hopper disabling percentage (disabling_perc) parameter.
    /// </summary>
    public SoftwareConfiguration SetDisablingPercentage(int percentage)
    {
        _values["disabling_perc"] = percentage;
        return this;
    }

    /// <summary>
    /// Limits the maximum number of coins dispensed in a change operation.
    /// </summary>
    public SoftwareConfiguration SetCoinsMaxNumber(int coins)
    {
        _values["coins_max_number"] = coins;
        return this;
    }

    internal IReadOnlyDictionary<string, int> Values => _values;
}
