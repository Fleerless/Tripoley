namespace Tripoley.Engine;

public enum RoundPhase
{
    // AwaitingOpeningLead = No card has been played yet
    AwaitingOpeningLead,
    // RunInProgress = The next legal card may be played
    RunInProgress,
    // RunStopped = The current player may lead a new suit if they have one
    RunStopped,
    // RoundComplete = Play is over, can happen with or without a winner
    RoundComplete
}

public class MichiganRound
{
    private readonly Deal _deal;
    public int CurrentLeaderIndex { get; private set; }
    public Card? LastPlayedCard { get; private set; }
    public int? WinnerIndex { get; private set; }
    public RoundPhase Phase { get; private set; } = RoundPhase.AwaitingOpeningLead;
    public bool IsAwaitingOpeningLead => Phase == RoundPhase.AwaitingOpeningLead;
    public bool IsRunInProgress => Phase == RoundPhase.RunInProgress;
    public bool IsRunStopped => Phase == RoundPhase.RunStopped;
    public bool IsRoundComplete => Phase == RoundPhase.RoundComplete;

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
    private void AdvanceAfterPlay(int playerIndex, Card playedCard)
    {
        LastPlayedCard = playedCard;
        if (CompleteIfHandEmpty(playerIndex))
        {
            return;
        }
        HandleRunStop(playerIndex, playedCard);
    }
    private void HandleRunStop(int playerIndex, Card lastPlayedCard)
    {
        if (RunEngine.IsRunStopped(lastPlayedCard, _deal.PlayerHands))
        {
            if (RunEngine.CanLeadAfterStop(GetPlayerHand(playerIndex), lastPlayedCard.Suit))
            {
                CurrentLeaderIndex = playerIndex;
                Phase = RoundPhase.RunStopped;
            }
            else if (RunEngine.FindNextLeaderIndex(_deal.PlayerHands, playerIndex, lastPlayedCard.Suit, out int nextLeaderIndex))
            {
                CurrentLeaderIndex = nextLeaderIndex;
                Phase = RoundPhase.RunStopped;
            }
            else
            {
                Phase = RoundPhase.RoundComplete;
                WinnerIndex = null;
            }
        }
        else
        {
            Phase = RoundPhase.RunInProgress;
        }
    }
    private bool CompleteIfHandEmpty(int playerIndex)
    {
        Hand hand = GetPlayerHand(playerIndex);
        if (hand.CardsRemaining <= 0)
        {
            Phase = RoundPhase.RoundComplete;
            WinnerIndex = playerIndex;
            return true;
        }
        return false;
    }
    private bool TryLeadOpeningCard(int playerIndex, Card cardToLead)
    {
        Hand hand = GetPlayerHand(playerIndex);

        if (
            Phase != RoundPhase.AwaitingOpeningLead || 
            playerIndex != CurrentLeaderIndex ||
            !RunEngine.CanLeadRun(hand, cardToLead)
        )
        {
            return false;
        }

        if (!hand.TryRemoveCard(cardToLead)) return false;
        
        AdvanceAfterPlay(playerIndex, cardToLead);

        return true;
    }
    private bool TryPlayNextCard(int playerIndex, Card cardToPlay)
    {
        Hand hand = GetPlayerHand(playerIndex);
        
        if (
            Phase != RoundPhase.RunInProgress ||
            LastPlayedCard is null ||
            !RunEngine.IsLegalNextCard(LastPlayedCard, cardToPlay)
        )
        {
            return false;
        }

        if (!hand.TryRemoveCard(cardToPlay)) return false;

        AdvanceAfterPlay(playerIndex, cardToPlay);
        
        return true;
    }
    private bool TryLeadCardAfterStop(int playerIndex, Card cardToLead)
    {
        Hand hand = GetPlayerHand(playerIndex);
        
        if (
            Phase != RoundPhase.RunStopped ||
            playerIndex != CurrentLeaderIndex ||
            LastPlayedCard is null ||
            !RunEngine.CanLeadWithCardAfterStop(hand, cardToLead, LastPlayedCard.Suit)
        )
        {
            return false;
        }

        if (!hand.TryRemoveCard(cardToLead)) return false;

        AdvanceAfterPlay(playerIndex, cardToLead);
        
        return true;
    }
    public bool TryPlay(int playerIndex, Card cardToPlay) => Phase switch
    {
        RoundPhase.AwaitingOpeningLead => TryLeadOpeningCard(playerIndex, cardToPlay),
        RoundPhase.RunInProgress => TryPlayNextCard(playerIndex, cardToPlay),
        RoundPhase.RunStopped => TryLeadCardAfterStop(playerIndex, cardToPlay),
        RoundPhase.RoundComplete => false,
        _ => throw new InvalidOperationException($"Unknown phase {Phase}")
    };
}