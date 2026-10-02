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
        List<Card> drawnCards = new List<Card>();
        
        while (deck.CardsRemaining > 0)
        {
            drawnCards.Add(deck.Draw());
        }

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

            while (deck.CardsRemaining >0)
            {
                deck.Draw();
            }
            
            Assert.Throws<InvalidOperationException>(() => deck.Draw());
    }
    [Fact]
    public void Shuffle_PreservesAllCards()
    {
        Deck deck = new Deck();
        List<Card> shuffeledCards = new List<Card>();

        deck.Shuffle();

        while (deck.CardsRemaining > 0)
        {
            shuffeledCards.Add(deck.Draw());
        }

        Assert.Equal(52, shuffeledCards.Count);
        Assert.Equal(52, shuffeledCards.Distinct().Count());
    }
    [Fact]
    public void Draw_ReturnsACard()
    {
        Deck deck = new Deck();

        Card card = deck.Draw();

        Assert.NotNull(card);
    }
    [Fact]
    public void Shuffle_DoesNotChangeCardCount()
    {
        Deck deck = new Deck();

        deck.Shuffle();

        Assert.Equal(52, deck.CardsRemaining);
    }
}
        
        
        
