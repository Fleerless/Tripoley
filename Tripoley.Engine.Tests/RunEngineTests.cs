namespace Tripoley.Engine.Tests;

public class RunEngineTests
{
    [Fact]
    public void RunEngine_IsLegalNextCard_ReturnsTrueForValidNextCard()
    {
        Card currentCard = new Card(Suit.Hearts, Rank.Two);
        Card candidateCard = new Card(Suit.Hearts, Rank.Three);

        Assert.True(RunEngine.IsLegalNextCard(currentCard, candidateCard));
    }
    [Fact]
    public void RunEngine_IsLegalNextCard_ReturnsTrueWhenKingIsFollowedByAce()
    {
        Card currentCard = new Card(Suit.Hearts, Rank.King);
        Card candidateCard = new Card(Suit.Hearts, Rank.Ace);

        Assert.True(RunEngine.IsLegalNextCard(currentCard, candidateCard));
    }
    [Fact]
    public void RunEngine_IsLegalNextCard_ReturnsFalseForRankInvalidNextCard()
    {
        Card currentCard = new Card(Suit.Clubs, Rank.Five);
        Card candidateCard = new Card(Suit.Clubs, Rank.Four);
        
        Assert.False(RunEngine.IsLegalNextCard(currentCard, candidateCard));
    }
    [Fact]
    public void RunEngine_IsLegalNextCard_ReturnsFalseWhenCandidateSkipsRank()
    {
        Card currentCard = new Card(Suit.Clubs, Rank.Two);
        Card candidateCard = new Card(Suit.Clubs, Rank.Four);

        Assert.False(RunEngine.IsLegalNextCard(currentCard, candidateCard));
    }
    [Fact]
    public void RunEngine_IsLegalNextCard_ReturnsFalseForSuitInvalidNextCard()
    {
        Card currentCard = new Card(Suit.Diamonds, Rank.Six);
        Card candidateCard = new Card(Suit.Spades, Rank.Seven);

        Assert.False(RunEngine.IsLegalNextCard(currentCard, candidateCard));
    }
    [Fact]
    public void RunEngine_IsRunStopped_ReturnsTrueWhenLastPlayedCardIsAce()
    {
        Card lastPlayedCard = new Card(Suit.Diamonds, Rank.Ace);
        List<Hand> activeHands = new List<Hand>();
        Hand widowHand = new Hand();

        Assert.True(RunEngine.IsRunStopped(lastPlayedCard, activeHands, widowHand));
    }
    [Fact]
    public void RunEngine_IsRunStopped_ReturnsTrueWhenNoActiveHandContainsNextCard()
    {
        Card lastPlayedCard = new Card(Suit.Spades, Rank.Eight);
        List<Hand> activeHands = new List<Hand>();
        Hand widowHand = new Hand();

        Assert.True(RunEngine.IsRunStopped(lastPlayedCard, activeHands, widowHand));
    }
    [Fact]
    public void RunEngine_IsRunStopped_ReturnsTrueWhenWidowHandContainsNextCard()
    {
        Card lastPlayedCard = new Card(Suit.Clubs, Rank.Nine);
        List<Hand> activeHands = new List<Hand>();
        Hand widowHand = new Hand();
        
        widowHand.AddCard(new Card(Suit.Clubs, Rank.Ten));

        Assert.True(RunEngine.IsRunStopped(lastPlayedCard, activeHands, widowHand));
    }
    [Fact]
    public void RunEngine_IsRunStopped_ReturnsFalseWhenNextCardIsInAnActiveHand()
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
    public void CanLeadAfterStop_WhenPreviousSuitIsRed_RequiresBlackCard(Suit previousSuit)
    {
        var hand = new Hand();
        hand.AddCard(new Card(Suit.Clubs, Rank.King));

        Assert.True(RunEngine.CanLeadAfterStop(hand, previousSuit));
    }

    [Theory]
    [InlineData(Suit.Clubs)]
    [InlineData(Suit.Spades)]
    public void CanLeadAfterStop_WhenPreviousSuitIsBlack_RequiresRedCard(Suit previousSuit)
    {
        var hand = new Hand();
        hand.AddCard(new Card(Suit.Hearts, Rank.Ace));

        Assert.True(RunEngine.CanLeadAfterStop(hand, previousSuit));
    }
    [Fact]
    public void IsValidLeadAfterStop_RejectsBlackCardWhenRedLeadRequired()
    {
        var hand = new Hand();
        var previousCard = new Card(Suit.Spades, Rank.Seven);
        var attemptedLeadCard = new Card(Suit.Clubs, Rank.Two);
        hand.AddCard(attemptedLeadCard);

        Assert.False(RunEngine.IsValidLeadAfterStop(hand, attemptedLeadCard, previousCard.Suit));
    }
    [Fact]
    public void IsRunStopped_WhenNextCardIsNotAvailableAnywhere_ReturnsTrue()
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
}