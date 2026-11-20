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
    public void ClaimChips(ChipPoolType type, Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        ChipPool pool = Get(type);
        int chips = pool.ChipCount;
        if (chips > 0)
        {
            pool.RemoveChips(chips);
            player.AddChips(chips);
        }
    }
    public void ClaimAll(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        foreach (ChipPoolType type in Enum.GetValues<ChipPoolType>())
        {
            ClaimChips(type, player);
        }
    }
    public void Scatter(int chips)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(chips);
        if (chips == 0) return;

        ChipPoolType[] types = Enum.GetValues<ChipPoolType>();
        int baseEach = chips / types.Length;
        int remainder = chips % types.Length;

        for (int index = 0; index < types.Length; index++)
        {
            int amount = baseEach + (index < remainder ? 1 : 0);
            if (amount > 0) _chipPools[types[index]].AddChips(amount);
        }
    }
}