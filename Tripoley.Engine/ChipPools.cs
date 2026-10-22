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
    private readonly Dictionary<ChipPoolType, ChipPool> _chipPools = [];
    public const int AnteAmount = 1;
    public int AnteCost => AnteAmount * _chipPools.Count;

    public ChipPools()
    {
        foreach (ChipPoolType type in Enum.GetValues<ChipPoolType>())
        {
            _chipPools[type] = new ChipPool(type);
        }
    }
    public ChipPool Get(ChipPoolType type) => _chipPools[type];

    public void AddAnte(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        
        if (!player.TryRemoveChips(AnteCost))
        {
            throw new InvalidOperationException("Player has insufficient chips to place ante.");
        }
        foreach (ChipPool pool in _chipPools.Values)
        {
            pool.AddChips(AnteAmount);
        } 
    }
}