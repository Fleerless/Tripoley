namespace Tripoley.Api.Projections;

using Tripoley.Engine;
using Tripoley.Api.Contracts;

public static class GameStateProjector
{
    public static GameStateView ToGameStateView(Game game)
    {
        var pools = Enum.GetValues<ChipPoolType>().Select(type => new PoolView(type, game.Pools.Get(type).ChipCount)).ToList();
        var round = game.CurrentMichiganRound;
        IReadOnlyList<PlayerSeatView> seats = game.CurrentDeal is null
            ? []
            : game.CurrentDeal.Seats.Select((seat, index) => new PlayerSeatView(index, seat.Player.Name, seat.Player.ChipCount, seat.Hand.CardsRemaining)).ToList();

        return new GameStateView(
            game.GamePhase,
            round?.CurrentLeaderIndex,
            round?.LastPlayedCard is { } card ? ToCardDto(card) : null,
            round?.WinnerIndex,
            pools, 
            seats);
    }
    public static SelfView ToSelfView(Game game, Guid viewerId)
    {
        var seat = game.CurrentDeal?.Seats.FirstOrDefault(seat => seat.Player.Id == viewerId);

        IReadOnlyList<CardDto> hand = seat is null
            ? []
            : seat.Hand.Cards.Select(ToCardDto).ToList();

        return new SelfView(hand);
    }
    public static CardDto ToCardDto(Card card)
    {
        return new CardDto(card.Suit, card.Rank);
    }
}