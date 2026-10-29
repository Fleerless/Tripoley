namespace Tripoley.Engine;

public class Player
{
    public int ChipCount { get; private set; }
    public Guid Id { get; }
    public string Name { get; }
    public bool IsReady { get; set; }
    public Player(int chipCount, string name)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(chipCount);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        ChipCount = chipCount;
        Id = Guid.NewGuid();
        Name = name;
    }
    public bool TryRemoveChips(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);

        if (ChipCount >= amount)
        {
            ChipCount = checked(ChipCount - amount);
            return true;
        }
        return false;
    }
    public void AddChips(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);

            ChipCount = checked(ChipCount + amount);
    }
}