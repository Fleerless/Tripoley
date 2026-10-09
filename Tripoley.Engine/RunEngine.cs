namespace Tripoley.Engine;

public static class RunEngine
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
        var nextCardInActiveHands = activeHands.Any(hand => hand.Contains(nextCard));

        return !nextCardInActiveHands || widowHand.Contains(nextCard);
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
    public static bool CanLeadAfterStop(Hand hand, Suit previousSuit)
    {
        return Enum.GetValues<Suit>()
            .Where(suit => suit != previousSuit)
            .Any(hand.HasCardOfSuit);
    }
    public static bool TryPlayNextCard(Hand hand, Card cardToPlay, Card currentCard)
    {
        if (!IsLegalNextCard(currentCard, cardToPlay))
        {
            return false;
        }
        return hand.TryRemoveCard(cardToPlay);
    }
    public static bool TryLeadRun(Hand hand, Card cardToLead)
    {
        if (hand.IsLowestCardOfSuit(cardToLead))
        {
            return hand.TryRemoveCard(cardToLead);
        }
        return false;
    }
    public static bool TryLeadAfterStop(Hand hand, Card cardToLead, Suit previousSuit)
    {
        bool cardNotPreviousSuit = cardToLead.Suit != previousSuit;
        if (cardNotPreviousSuit && hand.IsLowestCardOfSuit(cardToLead))
        {
            return hand.TryRemoveCard(cardToLead);
        }
        return false;
    }
    public static bool TryFindNextLeader(IReadOnlyList<Hand> playerHands, int playerWhoStoppedRunIndex, Suit previousSuit, out int nextLeaderIndex)
    {
        ArgumentNullException.ThrowIfNull(playerHands);

        int playerCount = playerHands.Count;

        if (playerWhoStoppedRunIndex < 0 || playerWhoStoppedRunIndex >= playerCount)
        {
            throw new ArgumentOutOfRangeException(nameof(playerWhoStoppedRunIndex));
        }

        nextLeaderIndex = default;

        for (int offset = 0; offset < playerCount; offset++)
        {
            int candidateIndex = (playerWhoStoppedRunIndex + offset) % playerCount;

            if (CanLeadAfterStop(playerHands[candidateIndex], previousSuit))
            {
                nextLeaderIndex = candidateIndex;
                return true;
            }
        }

        return false;
    }
}
