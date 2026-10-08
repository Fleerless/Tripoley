namespace Tripoley.Engine.Tests;

public class DealTests
{
    [Fact]
    public void Deal_EmptyDeck_ThrowsInvalidOperationException()
    {
        Deck deck = new Deck();

        while (deck.CardsRemaining > 0)
        {
            deck.Draw();
        }

        Assert.Throws<InvalidOperationException>(() => new Deal(deck, 2));
    }
    [Fact]
    public void Deal_ValidDeck_DistributesAllCardsAcrossPlayersAndWidow()
    {
        Deck deck = new Deck();
        deck.Shuffle();

        Deal deal = new Deal(deck, 2);

        int totalCards = deal.PlayerHands.Sum(hand => hand.CardsRemaining) + deal.Widow.CardsRemaining;

        Assert.Equal(52, totalCards);
        Assert.Equal(2, deal.PlayerHands.Count);
        Assert.Equal(18, deal.PlayerHands[0].CardsRemaining);
        Assert.Equal(17, deal.PlayerHands[1].CardsRemaining);
        Assert.Equal(17, deal.Widow.CardsRemaining);
    }
    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    public void Deal_ValidPlayerCounts_DistributeAllCardsAcrossPlayersAndWidowEvenly(int playerCount)
    {
        Deck deck = new Deck();
        Deal deal = new Deal(deck, playerCount);
        int totalCards = deal.Widow.CardsRemaining;
        int smallestHandSize = deal.Widow.CardsRemaining;
        int largestHandSize = deal.Widow.CardsRemaining;

        foreach (Hand hand in deal.PlayerHands)
        {
            int handSize = hand.CardsRemaining;
            totalCards += handSize;

            if (handSize < smallestHandSize)
            {
                smallestHandSize = handSize;
            }

            if (handSize > largestHandSize)
            {
                largestHandSize = handSize;
            }
        }

        Assert.Equal(playerCount, deal.PlayerHands.Count);
        Assert.Equal(52, totalCards);
        Assert.InRange(largestHandSize - smallestHandSize, 0, 1);
    }
    [Fact]
    public void Deal_PlayerCountBelowTwo_ThrowsArgumentOutOfRangeException()
    {
        Deck deck = new Deck();

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new Deal(deck, 1));
        
        Assert.Equal("playerCount", ex.ParamName);
        Assert.Contains("Player count must be between 2 and 6", ex.Message);
    }
    [Fact]
    public void Deal_PlayerCountAboveSix_ThrowsArgumentOutOfRangeException()
    {
        Deck deck = new Deck();

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new Deal(deck, 7));

        Assert.Equal("playerCount", ex.ParamName);
        Assert.Contains("Player count must be between 2 and 6", ex.Message);
    }
}