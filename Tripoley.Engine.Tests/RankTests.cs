namespace Tripoley.Engine.Tests;

public class RankTests
{
    [Theory]
    [InlineData(Rank.Two, 2)]
    [InlineData(Rank.Five, 5)]
    [InlineData(Rank.King, 13)]
    [InlineData(Rank.Ace, 14)]
    public void Rank_GetValue_UsesAceHighOrdering(Rank rank, int expected)
    {
        Assert.Equal(expected, rank.GetValue());
    }
}