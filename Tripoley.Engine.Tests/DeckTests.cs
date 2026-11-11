namespace Tripoley.Engine.Tests;

public class DeckTests
{
    [Fact]
    public void NewDeck_Has52Cards()
    {
        Deck deck = new Deck();

        Assert.Equal(52, deck.CardsRemaining);
    }
    [Fact]
    public void NewDeck_Has52UniqueCards()
    {
        Deck deck = new Deck();

        List<Card> drawnCards = DrawAll(deck);

        Assert.Equal(52, drawnCards.Distinct().Count());
    }
    [Fact]
    public void Draw_ReducesDeckSize()
    {
        Deck deck = new Deck();

        deck.Draw();

        Assert.Equal(51, deck.CardsRemaining);
    }
    [Fact]
    public void Draw_EmptyDeck_ThrowsException()
    {
        Deck deck = new Deck();
        DrawAll(deck);

        Assert.Throws<InvalidOperationException>(() => deck.Draw());
    }
    [Fact]
    public void Shuffle_PreservesAllCards()
    {
        Deck deck = new Deck();

        deck.Shuffle();
        List<Card> shuffledCards = DrawAll(deck);

        Assert.Equal(52, shuffledCards.Count);
        Assert.Equal(52, shuffledCards.Distinct().Count());
    }
    [Fact]
    public void Shuffle_ReordersTheCards()
    {
        List<Card> original = DrawAll(new Deck());

        Deck deck = new Deck();
        deck.Shuffle();
        List<Card> shuffled = DrawAll(deck);

        Assert.Equal(Sorted(original), Sorted(shuffled));
        Assert.NotEqual(original, shuffled);
    }
    [Fact]
    public void Shuffle_DoesNotChangeCardCount()
    {
        Deck deck = new Deck();

        deck.Shuffle();

        Assert.Equal(52, deck.CardsRemaining);
    }

    private static List<Card> DrawAll(Deck deck)
    {
        List<Card> cards = new List<Card>();
        while (deck.CardsRemaining > 0)
        {
            cards.Add(deck.Draw());
        }
        return cards;
    }

    private static IEnumerable<Card> Sorted(IEnumerable<Card> cards) =>
        cards.OrderBy(card => card.Suit).ThenBy(card => card.Rank);
}
