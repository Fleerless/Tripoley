namespace Tripoley.Api.Projections;

using Tripoley.Engine;
using Tripoley.Api.Contracts;

public static class GameStateProjector
{
    public static GameStateView ToGameStateView(Game game, string? lastRoundWinner = null)
    {
        var pools = Enum.GetValues<ChipPoolType>().Select(type => new PoolView(type, game.Pools.Get(type).ChipCount)).ToList();
        var round = game.GamePhase == GamePhase.GameComplete ? null : game.CurrentMichiganRound;
        IReadOnlyList<PlayerSeatView> seats = game.CurrentDeal is null
            ? []
            : game.CurrentDeal.Seats.Select((seat, index) => new PlayerSeatView(index, seat.Player.Name, seat.Player.ChipCount, seat.Hand.CardsRemaining)).ToList();

        return new GameStateView(
            game.GamePhase,
            round?.Phase,
            round?.CurrentLeaderIndex,
            CurrentTurnSeat(round, game.CurrentDeal),
            round?.LastPlayedCard is { } card ? ToCardDto(card) : null,
            NextCard(round),
            round?.WinnerIndex,
            lastRoundWinner,
            pools,
            seats);
    }

    private static int? CurrentTurnSeat(MichiganRound? round, Deal? deal)
    {
        if (round is null || deal is null) return null;
        return round.Phase switch
        {
            RoundPhase.AwaitingOpeningLead or RoundPhase.RunStopped => round.CurrentLeaderIndex,
            RoundPhase.RunInProgress when round.LastPlayedCard is { } last && last.Rank != Rank.Ace =>
                HolderOfNextCard(deal, new Card(last.Suit, last.Rank + 1)),
            _ => null,
        };
    }

    private static int? HolderOfNextCard(Deal deal, Card next)
    {
        for (int index = 0; index < deal.Seats.Count; index++)
            if (deal.Seats[index].Hand.Cards.Contains(next)) return index;
        return null;
    }

    private static CardDto? NextCard(MichiganRound? round) =>
        round is { Phase: RoundPhase.RunInProgress, LastPlayedCard: { } last } && last.Rank != Rank.Ace
            ? new CardDto(last.Suit, last.Rank + 1)
            : null;
    public static SelfView ToSelfView(Game game, Guid viewerId)
    {
        var deal = game.CurrentDeal;
        int seatIndex = deal is null ? -1 : IndexOfSeat(deal, viewerId);
        if (deal is null || seatIndex < 0) return new SelfView(null, [], []);

        var seat = deal.Seats[seatIndex];
        var hand = seat.Hand.Cards.Select(ToCardDto).ToList();
        var legalPlays = LegalPlays(game.CurrentMichiganRound, seat.Hand, seatIndex).Select(ToCardDto).ToList();
        return new SelfView(seatIndex, hand, legalPlays);
    }

    private static int IndexOfSeat(Deal deal, Guid viewerId)
    {
        for (int index = 0; index < deal.Seats.Count; index++)
            if (deal.Seats[index].Player.Id == viewerId) return index;
        return -1;
    }

    private static IEnumerable<Card> LegalPlays(MichiganRound? round, Hand hand, int seatIndex) =>
        round?.Phase switch
        {
            RoundPhase.AwaitingOpeningLead when seatIndex == round.CurrentLeaderIndex =>
                hand.Cards.Where(card => RunEngine.CanLeadRun(hand, card)),
            RoundPhase.RunStopped when seatIndex == round.CurrentLeaderIndex && round.LastPlayedCard is { } stop =>
                hand.Cards.Where(card => RunEngine.CanLeadWithCardAfterStop(hand, card, stop.Suit)),
            RoundPhase.RunInProgress when round.LastPlayedCard is { } last =>
                hand.Cards.Where(card => RunEngine.IsLegalNextCard(last, card)),
            _ => [],
        };
    public static CardDto ToCardDto(Card card)
    {
        return new CardDto(card.Suit, card.Rank);
    }
}