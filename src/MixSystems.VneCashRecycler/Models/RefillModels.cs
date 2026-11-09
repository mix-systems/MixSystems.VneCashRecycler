using MixSystems.VneCashRecycler.ValueObjects;

namespace MixSystems.VneCashRecycler.Models;

public sealed record RefillPollingResult(bool RefillInProgress, MoneyCents AmountInserted);

/// <summary>
/// Builder used to compose the payload for command tipo = 32 (manual refill).
/// </summary>
public sealed class ManualRefillRequest
{
    private readonly Dictionary<string, int> _values = new();

    /// <summary>
    /// Sets the number of notes contained in a recycler cassette (command tipo = 32).
    /// </summary>
    public ManualRefillRequest SetRecyclerCassette(int cassetteIndex, int pieces)
    {
        ValidateIndex(cassetteIndex);
        _values[$"cassette_refill_{cassetteIndex}"] = pieces;
        return this;
    }

    /// <summary>
    /// Sets the inventory for a hopper denomination in cents (command tipo = 32).
    /// </summary>
    public ManualRefillRequest SetHopperDenomination(int cents, int pieces)
    {
        ValidatePositive(cents);
        _values[$"hopper_refill_{cents}"] = pieces;
        return this;
    }

    /// <summary>
    /// Sets the number of notes in a dispenser cassette (command tipo = 32).
    /// </summary>
    public ManualRefillRequest SetDispenserCassette(int cassetteIndex, int pieces)
    {
        ValidateIndex(cassetteIndex);
        _values[$"disp_refill_{cassetteIndex}"] = pieces;
        return this;
    }

    /// <summary>
    /// Sets the content for a single-denomination coin dispenser hopper (command tipo = 32).
    /// </summary>
    public ManualRefillRequest SetCoinDispenserHopper(int hopperIndex, int pieces)
    {
        ValidateIndex(hopperIndex);
        _values[$"coinDisp_refill_{hopperIndex}"] = pieces;
        return this;
    }

    /// <summary>
    /// Allows specifying additional raw tag/value pairs for unsupported denominations.
    /// </summary>
    public ManualRefillRequest SetCustomValue(string tag, int pieces)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);
        _values[tag] = pieces;
        return this;
    }

    internal IReadOnlyDictionary<string, int> Values => _values;

    private static void ValidateIndex(int index)
    {
        if (index < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
    }

    private static void ValidatePositive(int value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }
    }
}
