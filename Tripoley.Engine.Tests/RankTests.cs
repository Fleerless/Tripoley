namespace Tripoley.Engine.Tests;

public class RankTests
{
    [Theory]
    [InlineData(Rank.Two, 2)]
    [InlineData(Rank.Three, 3)]
    [InlineData(Rank.Four, 4)]
    [InlineData(Rank.Six, 6)]
    [InlineData(Rank.Seven, 7)]
    [InlineData(Rank.Eight, 8)]
    [InlineData(Rank.Nine, 9)]
    [InlineData(Rank.Ten, 10)]
    [InlineData(Rank.Jack, 11)]
    [InlineData(Rank.Queen, 12)]
    [InlineData(Rank.Five, 5)]
    [InlineData(Rank.King, 13)]
    [InlineData(Rank.Ace, 14)]
    public void Rank_GetValue_UsesAceHighOrdering(Rank rank, int expected)
    {
        Assert.Equal(expected, rank.GetValue());
    }

    [Fact]
    public void Rank_GetValue_WhenRankIsInvalid_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ((Rank)(-1)).GetValue());
    }
}