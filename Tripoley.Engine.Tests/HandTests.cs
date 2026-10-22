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
    public void Cards_ExposesReadOnlyViewOfHand()
    {
        Hand hand = new Hand();
        Card card = new Card(Suit.Hearts, Rank.Ace);
        hand.AddCard(card);

        Assert.Equal(new[] { card }, hand.Cards);
        Assert.Throws<NotSupportedException>(() => ((IList<Card>)hand.Cards).Add(new Card(Suit.Clubs, Rank.Two)));
        Assert.Equal(1, hand.CardsRemaining);
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
    [Fact]
    public void TryRemoveCard_WhenCardIsInHand_RemovesItAndReturnsTrue()
    {
        Hand hand = new Hand();
        Card card = new Card(Suit.Hearts, Rank.Ace);
        hand.AddCard(card);

        Assert.True(hand.TryRemoveCard(card));

        Assert.False(hand.Contains(card));
        Assert.Equal(0, hand.CardsRemaining);
    }
    [Fact]
    public void TryRemoveCard_WhenCardIsNotInHand_ReturnsFalse()
    {
        Hand hand = new Hand();
        hand.AddCard(new Card(Suit.Hearts, Rank.Ace));

        Assert.False(hand.TryRemoveCard(new Card(Suit.Spades, Rank.Two)));
        Assert.Equal(1, hand.CardsRemaining);
    }
    [Fact]
    public void IsLowestCardOfSuit_WhenCardIsLowestOfItsSuit_ReturnsTrue()
    {
        Hand hand = new Hand();
        Card lowestHeart = new Card(Suit.Hearts, Rank.Four);
        hand.AddCard(lowestHeart);
        hand.AddCard(new Card(Suit.Hearts, Rank.Nine));
        hand.AddCard(new Card(Suit.Clubs, Rank.Two));

        Assert.True(hand.IsLowestCardOfSuit(lowestHeart));
    }
    [Fact]
    public void IsLowestCardOfSuit_WhenHandHoldsLowerCardOfSameSuit_ReturnsFalse()
    {
        Hand hand = new Hand();
        Card higherHeart = new Card(Suit.Hearts, Rank.Nine);
        hand.AddCard(new Card(Suit.Hearts, Rank.Four));
        hand.AddCard(higherHeart);

        Assert.False(hand.IsLowestCardOfSuit(higherHeart));
    }
    [Fact]
    public void IsLowestCardOfSuit_WhenCardIsNotInHand_ReturnsFalse()
    {
        Hand hand = new Hand();
        hand.AddCard(new Card(Suit.Hearts, Rank.Four));

        Assert.False(hand.IsLowestCardOfSuit(new Card(Suit.Hearts, Rank.Seven)));
    }
}