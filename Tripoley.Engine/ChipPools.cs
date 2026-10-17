namespace Tripoley.Engine;

public enum ChipPoolType
{
    AceOfHearts,
    KingOfHearts,
    QueenOfHearts,
    JackOfHearts,
    TenOfHearts,
    KingQueenOfHearts,
    EightNineTen,
    Kitty,
    Pot
}

public class ChipPools
{
    private readonly List<ChipPoolType> _chipPools = [];

    public ChipPools()
    {
        foreach (ChipPoolType type in Enum.GetValues<ChipPoolType>())
        {
            _chipPools.Add(type);
        }
    }
}