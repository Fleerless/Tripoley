namespace Tripoley.Engine.Tests;

public class PlayerTests
{
    [Fact]
    public void Constructor_SetsChipCountAndName()
    {
        Player player = new Player(chipCount: 50, name: "Jeff");

        Assert.Equal(50, player.ChipCount);
        Assert.Equal("Jeff", player.Name);
    }
    [Fact]
    public void Constructor_GeneratesUniqueIds()
    {
        Player first = new Player(chipCount: 50, name: "Jeff");
        Player second = new Player(chipCount: 50, name: "Jeff");

        Assert.NotEqual(Guid.Empty, first.Id);
        Assert.NotEqual(first.Id, second.Id);
    }
    [Fact]
    public void Constructor_NegativeChipCount_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Player(chipCount: -1, name: "Jeff"));
    }
    [Fact]
    public void Constructor_NullName_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Player(chipCount: 10, name: null!));
    }
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_BlankName_ThrowsArgumentException(string name)
    {
        Assert.Throws<ArgumentException>(() => new Player(chipCount: 10, name: name));
    }
    [Fact]
    public void TryRemoveChips_WithSufficientChips_RemovesThemAndReturnsTrue()
    {
        Player player = new Player(chipCount: 10, name: "Jeff");

        bool removed = player.TryRemoveChips(4);

        Assert.True(removed);
        Assert.Equal(6, player.ChipCount);
    }
    [Fact]
    public void TryRemoveChips_WithExactBalance_LeavesZero()
    {
        Player player = new Player(chipCount: 9, name: "Jeff");

        Assert.True(player.TryRemoveChips(9));
        Assert.Equal(0, player.ChipCount);
    }
    [Fact]
    public void TryRemoveChips_WithInsufficientChips_ReturnsFalseAndLeavesChipsUntouched()
    {
        Player player = new Player(chipCount: 3, name: "Jeff");

        bool removed = player.TryRemoveChips(4);

        Assert.False(removed);
        Assert.Equal(3, player.ChipCount);
    }
    [Fact]
    public void TryRemoveChips_NegativeAmount_ThrowsArgumentOutOfRangeException()
    {
        Player player = new Player(chipCount: 10, name: "Jeff");

        Assert.Throws<ArgumentOutOfRangeException>(() => player.TryRemoveChips(-1));
        Assert.Equal(10, player.ChipCount);
    }
    [Fact]
    public void AddChips_IncreasesChipCount()
    {
        Player player = new Player(chipCount: 10, name: "Jeff");

        player.AddChips(5);

        Assert.Equal(15, player.ChipCount);
    }
    [Fact]
    public void AddChips_ZeroChips_IsAllowed()
    {
        Player player = new Player(chipCount: 10, name: "Jeff");

        player.AddChips(0);

        Assert.Equal(10, player.ChipCount);
    }
    [Fact]
    public void AddChips_NegativeAmount_ThrowsArgumentOutOfRangeException()
    {
        Player player = new Player(chipCount: 10, name: "Jeff");

        Assert.Throws<ArgumentOutOfRangeException>(() => player.AddChips(-1));
        Assert.Equal(10, player.ChipCount);
    }
}
