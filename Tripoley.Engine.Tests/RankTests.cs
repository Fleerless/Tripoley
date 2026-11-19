namespace Tripoley.Engine.Tests;

public class RankTests
{
    [Theory]
    [InlineData(Rank.Two, 2)]
    [InlineData(Rank.King, 13)]
    [InlineData(Rank.Ace, 14)]
    public void Rank_UnderlyingValue_UsesAceHighOrdering(Rank rank, int expected)
    {
        Assert.Equal((int)rank, expected);
    }

    [Fact]
    public void Rank_Values_AreConsecutiveFromTwoThroughAce()
    {
        int[] values = Enum.GetValues<Rank>().Select(rank => (int)rank).Order().ToArray();

        Assert.Equal(Enumerable.Range(2, 13), values);
    }
}