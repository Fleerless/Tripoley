namespace Tripoley.Engine;

public class RunEngine
{
    public static bool IsLegalNextCard(Card currentCard, Card candidateCard)
    {
        if (currentCard.Suit != candidateCard.Suit)
        {
            return false;
        }   
        return RankExtensions.GetValue(candidateCard.Rank) == RankExtensions.GetValue(currentCard.Rank) + 1;
    }
    public static bool IsRunStopped(Card lastPlayedCard, IEnumerable<Hand> activeHands, Hand widowHand)
    {
        if (lastPlayedCard.Rank == Rank.Ace)
        {
            return true;
        }
        
        var nextCard = new Card(lastPlayedCard.Suit, GetNextRank(lastPlayedCard.Rank));

        return activeHands.All(hand => !hand.Contains(nextCard))
            && widowHand.Contains(nextCard);
    }
    private static Rank GetNextRank(Rank currentRank)
    {
        return currentRank switch
        {
            Rank.Two => Rank.Three,
            Rank.Three => Rank.Four,
            Rank.Four => Rank.Five,
            Rank.Five => Rank.Six,
            Rank.Six => Rank.Seven,
            Rank.Seven => Rank.Eight,
            Rank.Eight => Rank.Nine,
            Rank.Nine => Rank.Ten,
            Rank.Ten => Rank.Jack,
            Rank.Jack => Rank.Queen,
            Rank.Queen => Rank.King,
            Rank.King => Rank.Ace,
            Rank.Ace => throw new InvalidOperationException("Ace has no next rank in a run."),
            _ => throw new ArgumentOutOfRangeException(nameof(currentRank))
        };
    }
    private static bool IsSuitRed(Suit suit)
    {
        return suit == Suit.Hearts || suit == Suit.Diamonds;
    }
    private static bool RequiresRedLead(Suit previousSuit)
    {
        return !IsSuitRed(previousSuit);
    }
    public static bool CanLeadAfterStop(Hand hand, Suit previousSuit)
    {        
        if (RequiresRedLead(previousSuit))
        {
            return hand.HasCardOfSuit(Suit.Hearts) || hand.HasCardOfSuit(Suit.Diamonds);
        }
        else
        {
            return hand.HasCardOfSuit(Suit.Clubs) || hand.HasCardOfSuit(Suit.Spades);
        }
    }
    public static bool IsValidLeadAfterStop(Hand hand, Card playedCard, Suit previousSuit)
    {
        if (RequiresRedLead(previousSuit))
        {
            return hand.Contains(playedCard) && IsSuitRed(playedCard.Suit);
        }
        else
        {
            return hand.Contains(playedCard) && !IsSuitRed(playedCard.Suit);
        }
    }
}
