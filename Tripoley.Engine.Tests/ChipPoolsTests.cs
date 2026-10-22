namespace Tripoley.Engine.Tests;

public class ChipPoolsTests
{
    [Fact]
    public void Constructor_CreatesAnEmptyPoolForEverySpaceOnTheLayout()
    {
        ChipPools pools = new ChipPools();

        Assert.All(Enum.GetValues<ChipPoolType>(), type =>
        {
            ChipPool pool = pools.Get(type);
            Assert.Equal(type, pool.Type);
            Assert.Equal(0, pool.ChipCount);
        });
    }
    [Fact]
    public void AnteCost_IsOneChipPerLayoutSpace()
    {
        ChipPools pools = new ChipPools();

        // Pagat: each player antes one chip on each of the nine labelled spaces.
        Assert.Equal(9, pools.AnteCost);
        Assert.Equal(Enum.GetValues<ChipPoolType>().Length, pools.AnteCost);
    }
    [Fact]
    public void Get_ReturnsTheSamePoolInstanceEveryTime()
    {
        ChipPools pools = new ChipPools();

        Assert.Same(pools.Get(ChipPoolType.Pot), pools.Get(ChipPoolType.Pot));
    }
    [Fact]
    public void AddAnte_NullPlayer_ThrowsArgumentNullException()
    {
        ChipPools pools = new ChipPools();

        Assert.Throws<ArgumentNullException>(() => pools.AddAnte(null!));
    }
    [Fact]
    public void AddAnte_PlayerWithInsufficientChips_ThrowsAndLeavesPoolsUntouched()
    {
        ChipPools pools = new ChipPools();
        Player player = new Player(chipCount: 8, name: "Jeff");

        Assert.Throws<InvalidOperationException>(() => pools.AddAnte(player));

        Assert.Equal(8, player.ChipCount);
        Assert.All(Enum.GetValues<ChipPoolType>(), type => Assert.Equal(0, pools.Get(type).ChipCount));
    }
    [Fact]
    public void AddAnte_PlayerWithSufficientChips_PlacesOneChipOnEverySpace()
    {
        ChipPools pools = new ChipPools();
        Player player = new Player(chipCount: 9, name: "Jeff");

        pools.AddAnte(player);

        Assert.Equal(0, player.ChipCount);
        Assert.All(Enum.GetValues<ChipPoolType>(), type => Assert.Equal(1, pools.Get(type).ChipCount));
    }
    [Fact]
    public void AddAnte_MultipleAntes_AccumulateOnTopOfUnclaimedChips()
    {
        ChipPools pools = new ChipPools();

        pools.AddAnte(new Player(chipCount: 100, name: "Jeff"));
        pools.AddAnte(new Player(chipCount: 100, name: "Ann"));

        Assert.All(Enum.GetValues<ChipPoolType>(), type => Assert.Equal(2, pools.Get(type).ChipCount));
    }
}
