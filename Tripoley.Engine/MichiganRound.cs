namespace Tripoley.Engine;

public class MichiganRound
{
    private readonly Deal _deal;
    public int CurrentLeaderIndex { get; private set; }
    public Card? LastPlayedCard { get; private set; }
    public bool IsRunStopped { get; private set; }
    public bool IsRoundComplete { get; private set; }
    public int? WinnerIndex { get; private set; }

    public MichiganRound(Deal deal, int firstLeaderIndex)
    {
        ArgumentNullException.ThrowIfNull(deal);

        if (firstLeaderIndex < 0 || firstLeaderIndex >= deal.PlayerHands.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(firstLeaderIndex));
        }
        _deal = deal;
        CurrentLeaderIndex = firstLeaderIndex;
    }
    private Hand GetPlayerHand(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= _deal.PlayerHands.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(playerIndex));
        }

        return _deal.PlayerHands[playerIndex];
    }
    private bool CompleteIfHandEmpty(int playerIndex)
    {
        Hand hand = GetPlayerHand(playerIndex);
        if (hand.CardsRemaining <= 0)
        {
            IsRoundComplete = true;
            WinnerIndex = playerIndex;
            return true;
        }
        return false;
    }
    private bool HandleRunStop(int playerIndex)
    {
        if (LastPlayedCard is null)
        {
            return false;
        }

        if (RunEngine.IsRunStopped(LastPlayedCard, _deal.PlayerHands, _deal.Widow))
        {
            if (RunEngine.CanLeadAfterStop(GetPlayerHand(playerIndex), LastPlayedCard.Suit))
            {
                CurrentLeaderIndex = playerIndex;
                IsRunStopped = true;
                return true;
            }
            else if (RunEngine.TryFindNextLeader(_deal.PlayerHands, playerIndex, LastPlayedCard.Suit, out int nextLeaderIndex))
            {
                CurrentLeaderIndex = nextLeaderIndex;
                IsRunStopped = true;
                return true;
            }
            else
            {
                IsRoundComplete = true;
                IsRunStopped = true;
                WinnerIndex = null;
                return true;
            }
        }
        IsRunStopped = false;
        return false;
    }
    public bool TryLeadOpeningCard(int playerIndex, Card cardToLead)
    {
        Hand hand = GetPlayerHand(playerIndex);

        if (LastPlayedCard is not null || playerIndex != CurrentLeaderIndex || IsRoundComplete)
        {
            return false;
        }

        if (!RunEngine.TryLeadRun(hand, cardToLead))
        {
            return false;
        }

        LastPlayedCard = cardToLead;

        if (HandleRunStop(playerIndex))
        {
            return true;
        }

        return true;
    }
    public bool TryPlayNextCard(int playerIndex, Card cardToPlay)
    {
        Hand hand = GetPlayerHand(playerIndex);

        if (LastPlayedCard is null || IsRoundComplete)
        {
            return false;
        }

        if (!RunEngine.IsLegalNextCard(LastPlayedCard, cardToPlay))
        {
            return false;
        }

        if (!hand.TryRemoveCard(cardToPlay))
        {
            return false;
        }

        LastPlayedCard = cardToPlay;

        if (CompleteIfHandEmpty(playerIndex))
        {
            return true;
        }

        if (HandleRunStop(playerIndex))
        {
            return true;
        }

        return true;
    }
    public bool TryLeadCardAfterStop(int playerIndex, Card cardToLead)
    {
        Hand hand = GetPlayerHand(playerIndex);

        if (IsRoundComplete || playerIndex != CurrentLeaderIndex || !IsRunStopped || LastPlayedCard is null)
        {
            return false;
        }

        if (!RunEngine.TryLeadAfterStop(hand, cardToLead, LastPlayedCard.Suit))
        {
            return false;
        }

        LastPlayedCard = cardToLead;
        IsRunStopped = false;

        if (CompleteIfHandEmpty(playerIndex))
        {
            return true;
        }

        if (HandleRunStop(playerIndex))
        {
            return true;
        }

        return true;
    }
}