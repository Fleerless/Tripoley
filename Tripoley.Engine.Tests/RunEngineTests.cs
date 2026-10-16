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
        Hand activeHand = new Hand();
        activeHand.AddCard(new Card(Suit.Diamonds, Rank.Two));

        Assert.True(RunEngine.IsRunStopped(lastPlayedCard, [activeHand]));
    }
    [Fact]
    public void IsRunStopped_WhenNextCardIsNotInAnyActiveHand_ReturnsTrue()
    {
        Card lastPlayedCard = new Card(Suit.Spades, Rank.Eight);
        List<Hand> activeHands = new List<Hand>();

        Assert.True(RunEngine.IsRunStopped(lastPlayedCard, activeHands));
    }
    [Fact]
    public void IsRunStopped_WhenNextCardIsInAnActiveHand_ReturnsFalse()
    {
        Card lastPlayedCard = new Card(Suit.Diamonds, Rank.Jack);
        List<Hand> activeHands = new List<Hand>();

        Hand activeHand = new Hand();
        activeHand.AddCard(new Card(Suit.Diamonds, Rank.Queen));
        activeHands.Add(activeHand);

        Assert.False(RunEngine.IsRunStopped(lastPlayedCard, activeHands));
    }

    [Theory]
    [InlineData(Rank.Two, Rank.Three)]
    [InlineData(Rank.Three, Rank.Four)]
    [InlineData(Rank.Four, Rank.Five)]
    [InlineData(Rank.Five, Rank.Six)]
    [InlineData(Rank.Six, Rank.Seven)]
    [InlineData(Rank.Seven, Rank.Eight)]
    [InlineData(Rank.Eight, Rank.Nine)]
    [InlineData(Rank.Nine, Rank.Ten)]
    [InlineData(Rank.Ten, Rank.Jack)]
    [InlineData(Rank.Jack, Rank.Queen)]
    [InlineData(Rank.Queen, Rank.King)]
    [InlineData(Rank.King, Rank.Ace)]
    public void IsRunStopped_WhenNextRankIsInAnActiveHand_ReturnsFalse(Rank lastRank, Rank nextRank)
    {
        Card lastPlayedCard = new Card(Suit.Clubs, lastRank);
        Hand activeHand = new Hand();
        activeHand.AddCard(new Card(Suit.Clubs, nextRank));

        bool isStopped = RunEngine.IsRunStopped(lastPlayedCard, [activeHand]);

        Assert.False(isStopped);
    }

    [Fact]
    public void IsRunStopped_WhenLastPlayedRankIsInvalid_ThrowsArgumentOutOfRangeException()
    {
        Card lastPlayedCard = new Card(Suit.Clubs, (Rank)(-1));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => RunEngine.IsRunStopped(lastPlayedCard, []));
    }

    [Theory]
    [InlineData(Suit.Hearts)]
    [InlineData(Suit.Diamonds)]
    public void CanLeadAfterStop_WhenPreviousSuitIsHeartsOrDiamondsAndHandHasClubs_ReturnsTrue(Suit previousSuit)
    {
        var hand = new Hand();
        hand.AddCard(new Card(Suit.Clubs, Rank.King));

        Assert.True(RunEngine.CanLeadAfterStop(hand, previousSuit));
    }

    [Theory]
    [InlineData(Suit.Clubs)]
    [InlineData(Suit.Spades)]
    public void CanLeadAfterStop_WhenPreviousSuitIsClubsOrSpadesAndHandHasHearts_ReturnsTrue(Suit previousSuit)
    {
        var hand = new Hand();
        hand.AddCard(new Card(Suit.Hearts, Rank.Ace));

        Assert.True(RunEngine.CanLeadAfterStop(hand, previousSuit));
    }
    [Fact]
    public void CanLeadAfterStop_WhenHandOnlyHasPreviousSuit_ReturnsFalse()
    {
        var hand = new Hand();
        var previousCard = new Card(Suit.Spades, Rank.Five);
        hand.AddCard(new Card(Suit.Spades, Rank.Two));

        Assert.False(RunEngine.CanLeadAfterStop(hand, previousCard.Suit));
    }
    [Fact]
    public void CanLeadWithCardAfterStop_WhenLowestDifferentSuitOfSameColorIsPlayed_ReturnsTrue()
    {
        var hand = new Hand();
        var previousCard = new Card(Suit.Hearts, Rank.Ace);
        var lowestDiamond = new Card(Suit.Diamonds, Rank.Two);
        hand.AddCard(lowestDiamond);
        hand.AddCard(new Card(Suit.Diamonds, Rank.Six));

        Assert.True(RunEngine.CanLeadWithCardAfterStop(hand, lowestDiamond, previousCard.Suit));
        Assert.True(hand.Contains(lowestDiamond));
    }
    [Fact]
    public void CanLeadRun_WhenCardIsLowestInItsSuit_ReturnsTrue()
    {
        var hand = new Hand();
        var lowestHeart = new Card(Suit.Hearts, Rank.Seven);
        var higherHeart = new Card(Suit.Hearts, Rank.King);
        hand.AddCard(lowestHeart);
        hand.AddCard(higherHeart);

        Assert.True(RunEngine.CanLeadRun(hand, lowestHeart));
        Assert.True(hand.Contains(lowestHeart));
        Assert.True(hand.Contains(higherHeart));
    }

    [Fact]
    public void CanLeadRun_WhenCardIsNotLowestInItsSuit_ReturnsFalseAndLeavesHandUnchanged()
    {
        var hand = new Hand();
        var lowestHeart = new Card(Suit.Hearts, Rank.Six);
        var higherHeart = new Card(Suit.Hearts, Rank.Queen);
        hand.AddCard(lowestHeart);
        hand.AddCard(higherHeart);

        Assert.False(RunEngine.CanLeadRun(hand, higherHeart));
        Assert.True(hand.Contains(lowestHeart));
        Assert.True(hand.Contains(higherHeart));
    }

    [Fact]
    public void CanLeadRun_WhenCardIsNotInHand_ReturnsFalse()
    {
        var hand = new Hand();
        var heldHeart = new Card(Suit.Hearts, Rank.Five);
        var attemptedCard = new Card(Suit.Hearts, Rank.Jack);
        hand.AddCard(heldHeart);

        Assert.False(RunEngine.CanLeadRun(hand, attemptedCard));
        Assert.True(hand.Contains(heldHeart));
        Assert.Equal(1, hand.CardsRemaining);
    }
    [Fact]
    public void CanLeadWithCardAfterStop_WhenCardIsLowestInItsSuitAndValid_ReturnsTrue()
    {
        var hand = new Hand();
        var previousCard = new Card(Suit.Spades, Rank.Four);
        var lowestHeart = new Card(Suit.Hearts, Rank.Three);
        var higherHeart = new Card(Suit.Hearts, Rank.Ten);
        hand.AddCard(lowestHeart);
        hand.AddCard(higherHeart);

        Assert.True(RunEngine.CanLeadWithCardAfterStop(hand, lowestHeart, previousCard.Suit));
        Assert.True(hand.Contains(lowestHeart));
        Assert.True(hand.Contains(higherHeart));
    }
    [Fact]
    public void CanLeadWithCardAfterStop_WhenCardIsNotLowestInItsSuit_ReturnsFalse()
    {
        var hand = new Hand();
        var previousCard = new Card(Suit.Spades, Rank.Three);
        var lowestHeart = new Card(Suit.Hearts, Rank.Two);
        var higherHeart = new Card(Suit.Hearts, Rank.Nine);
        hand.AddCard(lowestHeart);
        hand.AddCard(higherHeart);

        Assert.False(RunEngine.CanLeadWithCardAfterStop(hand, higherHeart, previousCard.Suit));
        Assert.True(hand.Contains(lowestHeart));
        Assert.True(hand.Contains(higherHeart));
    }
    [Fact]
    public void CanLeadWithCardAfterStop_WhenCardIsSameSuit_ReturnsFalse()
    {
        var hand = new Hand();
        var previousCard = new Card(Suit.Spades, Rank.Ace);
        var attemptedSpade = new Card(Suit.Spades, Rank.Three);
        hand.AddCard(attemptedSpade);

        Assert.False(RunEngine.CanLeadWithCardAfterStop(hand, attemptedSpade, previousCard.Suit));
        Assert.True(hand.Contains(attemptedSpade));
    }

    [Fact]
    public void FindNextLeaderIndex_WhenStoppingPlayerCanLead_ReturnsThatPlayer()
    {
        List<Hand> playerHands = [new(), new(), new(), new(), new()];
        playerHands[2].AddCard(new Card(Suit.Clubs, Rank.Two));
        playerHands[3].AddCard(new Card(Suit.Diamonds, Rank.Two));

        bool foundLeader = RunEngine.FindNextLeaderIndex(playerHands, 2, Suit.Hearts, out int nextLeaderIndex);

        Assert.True(foundLeader);
        Assert.Equal(2, nextLeaderIndex);
    }

    [Fact]
    public void FindNextLeaderIndex_WhenStoppingPlayerCannotLead_ReturnsNextEligiblePlayerClockwise()
    {
        List<Hand> playerHands = [new(), new(), new(), new(), new()];
        playerHands[2].AddCard(new Card(Suit.Hearts, Rank.Two));
        playerHands[3].AddCard(new Card(Suit.Hearts, Rank.Three));
        playerHands[4].AddCard(new Card(Suit.Clubs, Rank.Two));
        playerHands[0].AddCard(new Card(Suit.Diamonds, Rank.Two));

        bool foundLeader = RunEngine.FindNextLeaderIndex(playerHands, 2, Suit.Hearts, out int nextLeaderIndex);

        Assert.True(foundLeader);
        Assert.Equal(4, nextLeaderIndex);
    }

    [Fact]
    public void FindNextLeaderIndex_WhenEligiblePlayerIsBeforeStoppingPlayer_WrapsAround()
    {
        List<Hand> playerHands = [new(), new(), new(), new(), new()];
        playerHands[0].AddCard(new Card(Suit.Clubs, Rank.Two));
        playerHands[2].AddCard(new Card(Suit.Hearts, Rank.Two));
        playerHands[3].AddCard(new Card(Suit.Hearts, Rank.Three));
        playerHands[4].AddCard(new Card(Suit.Hearts, Rank.Four));

        bool foundLeader = RunEngine.FindNextLeaderIndex(playerHands, 2, Suit.Hearts, out int nextLeaderIndex);

        Assert.True(foundLeader);
        Assert.Equal(0, nextLeaderIndex);
    }

    [Fact]
    public void FindNextLeaderIndex_WhenNoPlayerCanLead_ReturnsFalse()
    {
        List<Hand> playerHands = [new(), new(), new()];
        playerHands[0].AddCard(new Card(Suit.Hearts, Rank.Two));
        playerHands[1].AddCard(new Card(Suit.Hearts, Rank.Three));
        playerHands[2].AddCard(new Card(Suit.Hearts, Rank.Four));

        bool foundLeader = RunEngine.FindNextLeaderIndex(playerHands, 1, Suit.Hearts, out _);

        Assert.False(foundLeader);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    public void FindNextLeaderIndex_WhenStoppingPlayerIndexIsOutOfRange_Throws(int stoppingPlayerIndex)
    {
        List<Hand> playerHands = [new(), new(), new()];

        Assert.Throws<ArgumentOutOfRangeException>(
            () => RunEngine.FindNextLeaderIndex(playerHands, stoppingPlayerIndex, Suit.Hearts, out _));
    }

    [Fact]
    public void FindNextLeaderIndex_WhenPlayerHandsIsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => RunEngine.FindNextLeaderIndex(null!, 0, Suit.Hearts, out _));
    }
}