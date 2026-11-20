using Tripoley.Api.Contracts;
using Tripoley.Api.Projections;
using Tripoley.Engine;

namespace Tripoley.Api.Tests;

public class GameStateProjectorTests
{
    [Fact]
    public void ToGameStateView_OnAStartedGame_ReflectsPhasePoolsAndSeatCounts()
    {
        Game game = StartedGame(out Player[] players);

        GameStateView view = GameStateProjector.ToGameStateView(game);

        Assert.Equal(game.GamePhase, view.Phase);
        Assert.Equal(players.Length, view.Seats.Count);
        Assert.Equal(Enum.GetValues<ChipPoolType>().Length, view.Pools.Count);

        for (int index = 0; index < view.Seats.Count; index++)
            Assert.Equal(game.CurrentDeal!.Seats[index].Hand.CardsRemaining, view.Seats[index].CardsRemaining);
    }

    [Fact]
    public void ToGameStateView_MarksConnectedSeatsActiveAndDisconnectedSeatsAway()
    {
        Game game = StartedGame(out Player[] players);
        var connected = new Dictionary<Guid, bool>
        {
            [players[0].Id] = true,
            [players[1].Id] = false,
        };

        GameStateView view = GameStateProjector.ToGameStateView(game, connectedByPlayerId: connected);

        Assert.Equal(SeatPresence.Active, view.Seats.First(seat => seat.Name == players[0].Name).Presence);
        Assert.Equal(SeatPresence.Away, view.Seats.First(seat => seat.Name == players[1].Name).Presence);
    }

    [Fact]
    public void ToGameStateView_WithoutConnectionInfo_DefaultsSeatsToAway()
    {
        Game game = StartedGame(out _);

        GameStateView view = GameStateProjector.ToGameStateView(game);

        Assert.All(view.Seats, seat => Assert.Equal(SeatPresence.Away, seat.Presence));
    }

    [Fact]
    public void ToGameStateView_MarksAbandonedSeatsAbandoned_EvenWhenConnectedInfoIsAbsentOrStale()
    {
        Game game = StartedGame(out Player[] players);
        game.Abandon(players[0].Id);
        // players[0] abandoned (and no longer a member, so absent from the map); players[1] merely disconnected.
        var connected = new Dictionary<Guid, bool> { [players[1].Id] = false };

        GameStateView view = GameStateProjector.ToGameStateView(game, connectedByPlayerId: connected);

        Assert.Equal(SeatPresence.Abandoned, view.Seats.First(seat => seat.Name == players[0].Name).Presence);
        Assert.Equal(SeatPresence.Away, view.Seats.First(seat => seat.Name == players[1].Name).Presence);
    }

    [Fact]
    public void ToGameStateView_BeforeAnyDeal_HasEmptySeatsAndNullRoundFields()
    {
        Game game = new([new Player(100, "Jeff"), new Player(100, "Ann")]);

        GameStateView view = GameStateProjector.ToGameStateView(game);

        Assert.Equal(GamePhase.WaitingForDeal, view.Phase);
        Assert.Empty(view.Seats);
        Assert.Null(view.CurrentLeaderIndex);
        Assert.Null(view.LastPlayedCard);
        Assert.Null(view.WinnerIndex);
        Assert.Equal(Enum.GetValues<ChipPoolType>().Length, view.Pools.Count);
    }

    [Fact]
    public void ToSelfView_ReturnsOnlyTheViewersOwnHand()
    {
        Game game = StartedGame(out Player[] players);
        Player viewer = players[0];

        Seat seat = game.CurrentDeal!.Seats.First(seat => seat.Player.Id == viewer.Id);
        var expected = seat.Hand.Cards.Select(card => new CardDto(card.Suit, card.Rank)).ToList();

        SelfView self = GameStateProjector.ToSelfView(game, viewer.Id);

        Assert.Equal(expected, self.Hand);
    }

    [Fact]
    public void ToSelfView_ForSomeoneNotInTheDeal_ReturnsAnEmptyHand()
    {
        Game game = StartedGame(out _);

        SelfView self = GameStateProjector.ToSelfView(game, Guid.NewGuid());

        Assert.Empty(self.Hand);
    }

    [Fact]
    public void ToGameStateView_WhenGameComplete_ReportsNoActiveRoundOrSeats()
    {
        Game game = new([new Player(100, "Jeff"), new Player(5, "Broke Betty")]);
        game.StartNewDeal();
        Assert.Equal(GamePhase.GameComplete, game.GamePhase);

        GameStateView view = GameStateProjector.ToGameStateView(game);

        Assert.Equal(GamePhase.GameComplete, view.Phase);
        Assert.Empty(view.Seats);
        Assert.Null(view.CurrentLeaderIndex);
        Assert.Null(view.LastPlayedCard);
        Assert.Null(view.WinnerIndex);
    }

    private static Game StartedGame(out Player[] players)
    {
        players = [new Player(100, "Jeff"), new Player(100, "Ann")];
        Game game = new(players);
        game.StartNewDeal();
        game.StartMichiganRound();
        return game;
    }
}
