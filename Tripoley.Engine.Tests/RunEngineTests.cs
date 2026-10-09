namespace Tripoley.Engine.Tests;

public class RunEngineTests
{
    [Fact]
    public void IsLegalNextCard_WhenCandidateIsNextRankInSameSuit_ReturnsTrue()
    {
        Card currentCard = new Card(Suit.Hearts, Rank.Two);
        Card candidateCard = new Card(Suit.Hearts, Rank.Three);

        Assert.True(RunEngine.IsLegalNextCard(currentCard, candidateCard));
    }
    [Fact]
    public void IsLegalNextCard_WhenKingIsFollowedByAce_ReturnsTrue()
    {
        Card currentCard = new Card(Suit.Hearts, Rank.King);
        Card candidateCard = new Card(Suit.Hearts, Rank.Ace);

        Assert.True(RunEngine.IsLegalNextCard(currentCard, candidateCard));
    }
    [Fact]
    public void IsLegalNextCard_WhenCandidateRankIsLower_ReturnsFalse()
    {
        Card currentCard = new Card(Suit.Clubs, Rank.Five);
        Card candidateCard = new Card(Suit.Clubs, Rank.Four);
        
        Assert.False(RunEngine.IsLegalNextCard(currentCard, candidateCard));
    }
    [Fact]
    public void IsLegalNextCard_WhenCandidateSkipsRank_ReturnsFalse()
    {
        Card currentCard = new Card(Suit.Clubs, Rank.Two);
        Card candidateCard = new Card(Suit.Clubs, Rank.Four);

        Assert.False(RunEngine.IsLegalNextCard(currentCard, candidateCard));
    }
    [Fact]
    public void IsLegalNextCard_WhenCandidateSuitDiffers_ReturnsFalse()
    {
        Card currentCard = new Card(Suit.Diamonds, Rank.Six);
        Card candidateCard = new Card(Suit.Spades, Rank.Seven);

        Assert.False(RunEngine.IsLegalNextCard(currentCard, candidateCard));
    }
    [Fact]
    public void IsLegalNextCard_WhenLastCardIsAce_ReturnsFalseForAnyCandidate()
    {
        Card currentCard = new Card(Suit.Hearts, Rank.Ace);
        Card candidateCard = new Card(Suit.Hearts, Rank.Two);

        Assert.False(RunEngine.IsLegalNextCard(currentCard, candidateCard));
    }
    [Fact]
    public void IsRunStopped_WhenLastPlayedCardIsAce_ReturnsTrue()
    {
        Card lastPlayedCard = new Card(Suit.Diamonds, Rank.Ace);
        List<Hand> activeHands = new List<Hand>();
        Hand widowHand = new Hand();

        Assert.True(RunEngine.IsRunStopped(lastPlayedCard, activeHands, widowHand));
    }
    [Fact]
    public void IsRunStopped_WhenNextCardIsNotInAnyActiveHand_ReturnsTrue()
    {
        Card lastPlayedCard = new Card(Suit.Spades, Rank.Eight);
        List<Hand> activeHands = new List<Hand>();
        Hand widowHand = new Hand();

        Assert.True(RunEngine.IsRunStopped(lastPlayedCard, activeHands, widowHand));
    }
    [Fact]
    public void IsRunStopped_WhenNextCardIsInWidowHand_ReturnsTrue()
    {
        Card lastPlayedCard = new Card(Suit.Clubs, Rank.Nine);
        List<Hand> activeHands = new List<Hand>();
        Hand widowHand = new Hand();
        
        widowHand.AddCard(new Card(Suit.Clubs, Rank.Ten));

        Assert.True(RunEngine.IsRunStopped(lastPlayedCard, activeHands, widowHand));
    }
    [Fact]
    public void IsRunStopped_WhenNextCardIsInAnActiveHand_ReturnsFalse()
    {
        Card lastPlayedCard = new Card(Suit.Diamonds, Rank.Jack);
        List<Hand> activeHands = new List<Hand>();
        Hand widowHand = new Hand();

        Hand activeHand = new Hand();
        activeHand.AddCard(new Card(Suit.Diamonds, Rank.Queen));
        activeHands.Add(activeHand);

        Assert.False(RunEngine.IsRunStopped(lastPlayedCard, activeHands, widowHand));
    }
    [Theory]
    [InlineData(Suit.Hearts)]
    [InlineData(Suit.Diamonds)]
    public void CanLeadAfterStop_WhenPreviousSuitIsRedAndHandHasBlackCard_ReturnsTrue(Suit previousSuit)
    {
        var hand = new Hand();
        hand.AddCard(new Card(Suit.Clubs, Rank.King));

        Assert.True(RunEngine.CanLeadAfterStop(hand, previousSuit));
    }

    [Theory]
    [InlineData(Suit.Clubs)]
    [InlineData(Suit.Spades)]
    public void CanLeadAfterStop_WhenPreviousSuitIsBlackAndHandHasRedCard_ReturnsTrue(Suit previousSuit)
    {
        var hand = new Hand();
        hand.AddCard(new Card(Suit.Hearts, Rank.Ace));

        Assert.True(RunEngine.CanLeadAfterStop(hand, previousSuit));
    }
    [Fact]
    public void CanLeadAfterStop_WhenHandHasNoCardOfRequiredColor_ReturnsFalse()
    {
        var hand = new Hand();
        var previousCard = new Card(Suit.Spades, Rank.Five);
        var attemptedLeadCard = new Card(Suit.Clubs, Rank.Two);
        hand.AddCard(attemptedLeadCard);

        Assert.False(RunEngine.CanLeadAfterStop(hand, previousCard.Suit));
    }
    [Fact]
    public void IsValidLeadAfterStop_WhenBlackCardIsPlayedAndRedIsRequired_ReturnsFalse()
    {
        var hand = new Hand();
        var previousCard = new Card(Suit.Spades, Rank.Seven);
        var attemptedLeadCard = new Card(Suit.Clubs, Rank.Two);
        hand.AddCard(attemptedLeadCard);

        Assert.False(RunEngine.IsValidLeadAfterStop(hand, attemptedLeadCard, previousCard.Suit));
    }
    [Fact]
    public void IsRunStopped_WhenNextCardIsUnavailableAnywhere_ReturnsTrue()
    {
        var lastPlayedCard = new Card(Suit.Spades, Rank.Eight);

        var firstHand = new Hand();
        var secondHand = new Hand();

        var activeHands = new List<Hand>
        {
            firstHand,
            secondHand
        };

        var widowHand = new Hand();

        Assert.True(RunEngine.IsRunStopped(lastPlayedCard, activeHands, widowHand));
    }
    [Fact]
    public void TryPlayNextCard_WhenCardIsLegalAndInHand_RemovesCardAndReturnsTrue()
    {
        var hand = new Hand();
        var currentCard = new Card(Suit.Hearts, Rank.Five);
        var nextCard = new Card(Suit.Hearts, Rank.Six);
        hand.AddCard(nextCard);

        var result = RunEngine.TryPlayNextCard(hand, nextCard, currentCard);

        Assert.True(result);
        Assert.False(hand.Contains(nextCard));
    }
    [Fact] 
    public void TryPlayNextCard_WhenCardIsNotLegal_ReturnsFalseAndLeavesHandUnchanged()
    {
        var hand = new Hand();
        var currentCard = new Card(Suit.Diamonds, Rank.Three);
        var illegalCard = new Card(Suit.Diamonds, Rank.Five);
        hand.AddCard(illegalCard);

        var result = RunEngine.TryPlayNextCard(hand, illegalCard, currentCard);

        Assert.False(result);
        Assert.True(hand.Contains(illegalCard));
    }
    [Fact] 
    public void TryPlayNextCard_WhenCardIsNotInHand_ReturnsFalseAndLeavesHandUnchanged()
    {
        var hand = new Hand();
        var currentCard = new Card(Suit.Clubs, Rank.Two);
        var nextCard = new Card(Suit.Clubs, Rank.Four);

        var result = RunEngine.TryPlayNextCard(hand, nextCard, currentCard);

        Assert.False(result);
        Assert.Equal(0, hand.CardsRemaining);
    }
    [Fact]
    public void TryLeadRun_WhenCardIsLowestInItsSuit_ReturnsTrueAndRemovesCard()
    {
        var hand = new Hand();
        var lowestHeart = new Card(Suit.Hearts, Rank.Seven);
        var higherHeart = new Card(Suit.Hearts, Rank.King);
        hand.AddCard(lowestHeart);
        hand.AddCard(higherHeart);

        var result = RunEngine.TryLeadRun(hand, lowestHeart);

        Assert.True(result);
        Assert.False(hand.Contains(lowestHeart));
        Assert.True(hand.Contains(higherHeart));
    }

    [Fact]
    public void TryLeadRun_WhenCardIsNotLowestInItsSuit_ReturnsFalseAndLeavesHandUnchanged()
    {
        var hand = new Hand();
        var lowestHeart = new Card(Suit.Hearts, Rank.Six);
        var higherHeart = new Card(Suit.Hearts, Rank.Queen);
        hand.AddCard(lowestHeart);
        hand.AddCard(higherHeart);

        var result = RunEngine.TryLeadRun(hand, higherHeart);

        Assert.False(result);
        Assert.True(hand.Contains(lowestHeart));
        Assert.True(hand.Contains(higherHeart));
    }

    [Fact]
    public void TryLeadRun_WhenCardIsNotInHand_ReturnsFalseAndLeavesHandUnchanged()
    {
        var hand = new Hand();
        var heldHeart = new Card(Suit.Hearts, Rank.Five);
        var attemptedCard = new Card(Suit.Hearts, Rank.Jack);
        hand.AddCard(heldHeart);

        var result = RunEngine.TryLeadRun(hand, attemptedCard);

        Assert.False(result);
        Assert.True(hand.Contains(heldHeart));
        Assert.Equal(1, hand.CardsRemaining);
    }
    [Fact]
    public void TryLeadAfterStop_WhenCardIsLowestInItsSuitAndValid_ReturnsTrueAndRemovesCard()
    {
        var hand = new Hand();
        var previousCard = new Card(Suit.Spades, Rank.Four);
        var lowestHeart = new Card(Suit.Hearts, Rank.Three);
        var higherHeart = new Card(Suit.Hearts, Rank.Ten);
        hand.AddCard(lowestHeart);
        hand.AddCard(higherHeart);

        var result = RunEngine.TryLeadAfterStop(hand, lowestHeart, previousCard.Suit);

        Assert.True(result);
        Assert.False(hand.Contains(lowestHeart));
        Assert.True(hand.Contains(higherHeart));
    }
    [Fact]
    public void TryLeadAfterStop_WhenCardIsNotLowestInItsSuit_ReturnsFalseAndLeavesHandUnchanged()
    {
        var hand = new Hand();
        var previousCard = new Card(Suit.Spades, Rank.Three);
        var lowestHeart = new Card(Suit.Hearts, Rank.Two);
        var higherHeart = new Card(Suit.Hearts, Rank.Nine);
        hand.AddCard(lowestHeart);
        hand.AddCard(higherHeart);

        var result = RunEngine.TryLeadAfterStop(hand, higherHeart, previousCard.Suit);

        Assert.False(result);
        Assert.True(hand.Contains(lowestHeart));
        Assert.True(hand.Contains(higherHeart));
    }
    [Fact]
    public void TryLeadAfterStop_WhenCardIsWrongColor_ReturnsFalseAndLeavesHandUnchanged()
    {
        var hand = new Hand();
        var previousCard = new Card(Suit.Spades, Rank.Ace);
        var attemptedClub = new Card(Suit.Clubs, Rank.Three);
        hand.AddCard(attemptedClub);

        var result = RunEngine.TryLeadAfterStop(hand, attemptedClub, previousCard.Suit);

        Assert.False(result);
        Assert.True(hand.Contains(attemptedClub));
    }
}