namespace Tripoley.Engine.Tests;

public class DealTests
{
    [Fact]
    public void Deal_NullDeck_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Deal.Create(null!, CreatePlayers(2)));
    }

    [Fact]
    public void Deal_NullPlayers_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Deal.Create(new Deck(), null!));
    }

    [Fact]
    public void Deal_EmptyDeck_ThrowsInvalidOperationException()
    {
        Deck deck = new Deck();

        while (deck.CardsRemaining > 0)
        {
            deck.Draw();
        }

        Assert.Throws<InvalidOperationException>(() => Deal.Create(deck, CreatePlayers(2)));
    }
    [Fact]
    public void Deal_ValidDeck_DistributesAllCardsAcrossPlayersAndWidow()
    {
        Deck deck = new Deck();
        deck.Shuffle();

        Deal deal = Deal.Create(deck, CreatePlayers(2));

        int totalCards = deal.Seats.Sum(seat => seat.Hand.CardsRemaining) + deal.Widow.CardsRemaining;

        Assert.Equal(52, totalCards);
        Assert.Equal(2, deal.Seats.Count);
        Assert.Equal(18, deal.Seats[0].Hand.CardsRemaining);
        Assert.Equal(17, deal.Seats[1].Hand.CardsRemaining);
        Assert.Equal(17, deal.Widow.CardsRemaining);
    }
    [Fact]
    public void Deal_PreservesDealOrderAndPlayerIdentity()
    {
        Player[] players = CreatePlayers(3);

        Deal deal = Deal.Create(new Deck(), players);

        Assert.Equal(players, deal.Seats.Select(seat => seat.Player));
    }
    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    public void Deal_ValidPlayerCounts_DistributeAllCardsAcrossPlayersAndWidowEvenly(int playerCount)
    {
        Deck deck = new Deck();
        Deal deal = Deal.Create(deck, CreatePlayers(playerCount));
        int totalCards = deal.Widow.CardsRemaining;
        int smallestHandSize = deal.Widow.CardsRemaining;
        int largestHandSize = deal.Widow.CardsRemaining;

        foreach (Seat seat in deal.Seats)
        {
            int handSize = seat.Hand.CardsRemaining;
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

        Assert.Equal(playerCount, deal.Seats.Count);
        Assert.Equal(52, totalCards);
        Assert.InRange(largestHandSize - smallestHandSize, 0, 1);
    }
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    public void Deal_InvalidPlayerCount_ThrowsArgumentException(int playerCount)
    {
        Deck deck = new Deck();

        var ex = Assert.ThrowsAny<ArgumentException>(() => Deal.Create(deck, CreatePlayers(playerCount)));
        
        Assert.Equal("playersInDealOrder", ex.ParamName);
    }

    private static Player[] CreatePlayers(int count)
    {
        return Enumerable
            .Range(1, count)
            .Select(index => new Player(chipCount: 100, name: $"Player {index}"))
            .ToArray();
    }
}