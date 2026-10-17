namespace Tripoley.Engine;

public sealed class ChipPool
{
    public ChipPoolType Type { get; }
    public int ChipCount { get; private set; }

    public ChipPool(ChipPoolType type)
    {
        Type = type;
    }

    public void AddChips(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        ChipCount = checked(ChipCount + amount);
    }

    public void RemoveChips(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        
        if (amount > ChipCount)
        {
            throw new InvalidOperationException("Cannot remove more chips than are available.");
        }
        ChipCount = checked(ChipCount - amount);
    }
}