namespace Tripoley.Engine.Tests;

public class HandTests
{
    [Fact]
    public void NewHand_HasZeroCards()
    {
        Hand hand = new Hand();

        Assert.Equal(0, hand.CardsRemaining);
    }
    [Fact]
    public void AddCard_IncreasesCardCount()
    {
        Hand hand = new Hand();
        Card card = new Card(Suit.Hearts, Rank.Ace);
        
        hand.AddCard(card);
        
        Assert.Equal(1, hand.CardsRemaining);
    }
    [Fact]
    public void Contains_Returns_TrueForCardInHand()
    {
        Hand hand = new Hand();
        Card card = new Card(Suit.Hearts, Rank.Ace);

        hand.AddCard(card);

        Assert.True(hand.Contains(card));
    }
    [Fact]
    public void Contains_Returns_FalseForCardNotInHand()
    {
        Hand hand = new Hand();
        Card card = new Card(Suit.Hearts, Rank.Ace);
        Card otherCard = new Card(Suit.Spades, Rank.Two);

        hand.AddCard(card);

        Assert.False(hand.Contains(otherCard));
    }
    [Fact]
    public void HasCardOfSuit_ReturnsTrueWhenHandContainsSuit()
    {
        Hand hand = new Hand();
        hand.AddCard(new Card(Suit.Hearts, Rank.Five));

        Assert.True(hand.HasCardOfSuit(Suit.Hearts));
    }
    [Fact]
    public void HasCardOfSuit_ReturnsFalseWhenHandDoesNotContainSuit()
    {
        Hand hand = new Hand();
        hand.AddCard(new Card(Suit.Hearts, Rank.Five));

        Assert.False(hand.HasCardOfSuit(Suit.Spades));
    }
}