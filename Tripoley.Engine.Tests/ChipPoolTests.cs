namespace Tripoley.Engine.Tests;

public class ChipPoolTests
{
    [Fact]
    public void Constructor_SetsTypeAndStartsEmpty()
    {
        ChipPool pool = new ChipPool(ChipPoolType.Kitty);

        Assert.Equal(ChipPoolType.Kitty, pool.Type);
        Assert.Equal(0, pool.ChipCount);
    }
    [Fact]
    public void AddChips_IncreasesChipCount()
    {
        ChipPool pool = new ChipPool(ChipPoolType.Pot);

        pool.AddChips(5);
        pool.AddChips(3);

        Assert.Equal(8, pool.ChipCount);
    }
    [Fact]
    public void AddChips_NegativeAmount_ThrowsArgumentOutOfRangeException()
    {
        ChipPool pool = new ChipPool(ChipPoolType.Pot);

        Assert.Throws<ArgumentOutOfRangeException>(() => pool.AddChips(-1));
        Assert.Equal(0, pool.ChipCount);
    }
    [Fact]
    public void RemoveChips_DecreasesChipCount()
    {
        ChipPool pool = new ChipPool(ChipPoolType.Pot);
        pool.AddChips(10);

        pool.RemoveChips(4);

        Assert.Equal(6, pool.ChipCount);
    }
    [Fact]
    public void RemoveChips_ExactAmount_EmptiesPool()
    {
        ChipPool pool = new ChipPool(ChipPoolType.Pot);
        pool.AddChips(7);

        pool.RemoveChips(7);

        Assert.Equal(0, pool.ChipCount);
    }
    [Fact]
    public void RemoveChips_MoreThanAvailable_ThrowsAndLeavesPoolUntouched()
    {
        ChipPool pool = new ChipPool(ChipPoolType.Pot);
        pool.AddChips(3);

        Assert.Throws<InvalidOperationException>(() => pool.RemoveChips(4));
        Assert.Equal(3, pool.ChipCount);
    }
    [Fact]
    public void RemoveChips_NegativeAmount_ThrowsArgumentOutOfRangeException()
    {
        ChipPool pool = new ChipPool(ChipPoolType.Pot);
        pool.AddChips(3);

        Assert.Throws<ArgumentOutOfRangeException>(() => pool.RemoveChips(-1));
        Assert.Equal(3, pool.ChipCount);
    }
}
