using Microsoft.Extensions.Time.Testing;
using Tripoley.Api.Contracts;
using Tripoley.Api.Projections;
using Tripoley.Api.Rooms;
using Tripoley.Engine;

namespace Tripoley.Api.Tests;

public class RoomTests
{
    [Fact]
    public void AddMember_InLobby_AddsAndReturnsTheStoredMember()
    {
        Room room = new("ABCD");

        RoomMember? member = room.AddMember(userId: "user-1", name: "Jeff");

        Assert.NotNull(member);
        Assert.Equal("user-1", member.UserId);
        Assert.Equal("Jeff", member.Player.Name);
        Assert.False(member.IsReady);
        Assert.True(member.IsConnected);
        Assert.Same(member, room.FindMember("user-1"));
    }

    [Fact]
    public void AddMember_GivesEachPlayerTheStartingChips()
    {
        Room room = new("ABCD");

        RoomMember member = room.AddMember("user-1", "Jeff")!;

        Assert.Equal(Game.StartingChips, member.Player.ChipCount);
    }

    [Fact]
    public void AddMember_BeyondMaxPlayerCount_ReturnsNull()
    {
        Room room = new("ABCD");
        for (int seat = 0; seat < Game.MaxPlayerCount; seat++)
            Assert.NotNull(room.AddMember($"user-{seat}", $"Player {seat}"));

        RoomMember? overflow = room.AddMember("user-extra", "Latecomer");

        Assert.Null(overflow);
    }

    [Fact]
    public void AddMember_WhenNotInLobby_ReturnsNull()
    {
        Room room = CreateStartedRoom();

        Assert.Null(room.AddMember("user-late", "Latecomer"));
    }

    [Fact]
    public void SetReady_TogglesTheStoredMember()
    {
        Room room = new("ABCD");
        room.AddMember("user-1", "Jeff");

        bool set = room.SetReady("user-1", true);

        Assert.True(set);
        Assert.True(room.FindMember("user-1")!.IsReady);
    }

    [Fact]
    public void SetReady_UnknownUser_ReturnsFalse()
    {
        Room room = new("ABCD");

        Assert.False(room.SetReady("user-unknown", true));
    }

    [Fact]
    public void MarkDisconnected_KeepsTheSeatButFlagsItAway()
    {
        Room room = new("ABCD");
        room.AddMember("user-1", "Jeff");

        RoomMember? member = room.MarkDisconnected("user-1");

        Assert.NotNull(member);
        Assert.False(member.IsConnected);
        Assert.Same(member, room.FindMember("user-1"));
    }

    [Fact]
    public void MarkDisconnected_UnknownUser_ReturnsNull()
    {
        Room room = new("ABCD");

        Assert.Null(room.MarkDisconnected("user-unknown"));
    }

    [Fact]
    public void MarkConnected_ReclaimsAnAwaySeat()
    {
        Room room = new("ABCD");
        room.AddMember("user-1", "Jeff");
        room.MarkDisconnected("user-1");

        RoomMember? member = room.MarkConnected("user-1");

        Assert.NotNull(member);
        Assert.True(member.IsConnected);
    }

    [Fact]
    public void Room_IsAbandoned_OnlyOnceEveryMemberIsAwayAndPastTheCutoff()
    {
        FakeTimeProvider time = new();
        Room room = new("ABCD", timeProvider: time);
        room.AddMember("user-1", "Jeff");
        room.AddMember("user-2", "Ann");

        room.MarkDisconnected("user-1");
        Assert.False(room.IsAbandonedSince(time.GetUtcNow()));

        room.MarkDisconnected("user-2");
        DateTimeOffset wentIdle = time.GetUtcNow();
        time.Advance(TimeSpan.FromMinutes(31));

        Assert.False(room.IsAbandonedSince(wentIdle - TimeSpan.FromMinutes(1)));
        Assert.True(room.IsAbandonedSince(time.GetUtcNow() - TimeSpan.FromMinutes(30)));
    }

    [Fact]
    public void Room_Reconnecting_ClearsTheAbandonedClock()
    {
        FakeTimeProvider time = new();
        Room room = new("ABCD", timeProvider: time);
        room.AddMember("user-1", "Jeff");
        room.MarkDisconnected("user-1");
        time.Advance(TimeSpan.FromHours(1));

        room.MarkConnected("user-1");

        Assert.False(room.IsAbandonedSince(time.GetUtcNow()));
    }

    [Fact]
    public void StartGame_WhenAllReadyAndEnoughPlayers_BeginsTheGame()
    {
        Room room = new("ABCD");
        AddReadyMember(room, "user-1", "Jeff");
        AddReadyMember(room, "user-2", "Ann");

        bool started = room.StartGame();

        Assert.True(started);
        Assert.Equal(RoomState.InProgress, room.State);
        Assert.NotNull(room.Game);
    }

    [Fact]
    public void StartGame_WithFewerThanMinPlayers_ReturnsFalseAndStaysInLobby()
    {
        Room room = new("ABCD");
        AddReadyMember(room, "user-1", "Jeff");

        bool started = room.StartGame();

        Assert.False(started);
        Assert.Equal(RoomState.Lobby, room.State);
        Assert.Null(room.Game);
    }

    [Fact]
    public void StartGame_WhenNotEveryoneIsReady_ReturnsFalseAndStaysInLobby()
    {
        Room room = new("ABCD");
        AddReadyMember(room, "user-1", "Jeff");
        room.AddMember("user-2", "Ann");

        bool started = room.StartGame();

        Assert.False(started);
        Assert.Equal(RoomState.Lobby, room.State);
        Assert.Null(room.Game);
    }

    [Fact]
    public void StartGame_WhenAlreadyInProgress_ReturnsFalse()
    {
        Room room = CreateStartedRoom();

        Assert.False(room.StartGame());
    }

    [Fact]
    public void StartGame_WhenAReadyMemberIsAway_ReturnsFalseAndStaysInLobby()
    {
        Room room = new("ABCD");
        AddReadyMember(room, "user-1", "Jeff");
        AddReadyMember(room, "user-2", "Ann");
        room.MarkDisconnected("user-2");

        bool started = room.StartGame();

        Assert.False(started);
        Assert.Equal(RoomState.Lobby, room.State);
    }

    [Fact]
    public void StartGame_AfterAnAwayMemberReconnects_BeginsTheGame()
    {
        Room room = new("ABCD");
        AddReadyMember(room, "user-1", "Jeff");
        AddReadyMember(room, "user-2", "Ann");
        room.MarkDisconnected("user-2");
        room.MarkConnected("user-2");

        Assert.True(room.StartGame());
        Assert.Equal(RoomState.InProgress, room.State);
    }

    [Fact]
    public void TryPlayCard_WhenStillInLobby_ReturnsFalse()
    {
        Room room = new("ABCD");
        room.AddMember("user-1", "Jeff");

        Assert.False(room.TryPlayCard("user-1", new Card(Suit.Hearts, Rank.Two)));
    }

    [Fact]
    public void TryPlayCard_FromAnUnknownUser_ReturnsFalse()
    {
        Room room = CreateStartedRoom();

        Assert.False(room.TryPlayCard("user-unknown", new Card(Suit.Hearts, Rank.Two)));
    }

    [Fact]
    public void TryPlayCard_WhenTheLeaderPlaysALegalOpeningCard_ReturnsTrue()
    {
        Room room = CreateStartedRoom();
        Game game = room.Game!;

        int leaderIndex = game.CurrentMichiganRound!.CurrentLeaderIndex;
        Seat leaderSeat = game.CurrentDeal!.Seats[leaderIndex];

        Card legalLead = leaderSeat.Hand.Cards.OrderBy(card => card.Suit).ThenBy(card => card.Rank).First();
        string leaderUser = room.Members()
            .First(member => member.Player.Id == leaderSeat.Player.Id).UserId;

        Assert.True(room.TryPlayCard(leaderUser, legalLead));
    }

    [Fact]
    public void AdvanceToNextDeal_WhenStillInLobby_ReturnsFalse()
    {
        Room room = new("ABCD");
        room.AddMember("user-1", "Jeff");

        Assert.False(room.AdvanceToNextDeal());
        Assert.Equal(RoomState.Lobby, room.State);
    }

    [Fact]
    public void AdvanceToNextDeal_MidRound_ReturnsFalseAndLeavesTheRoundRunning()
    {
        Room room = CreateStartedRoom();

        bool advanced = room.AdvanceToNextDeal();

        Assert.False(advanced);
        Assert.Equal(RoomState.InProgress, room.State);
        Assert.Equal(GamePhase.MichiganRummyInProgress, room.Game!.GamePhase);
    }

    [Fact]
    public void AdvanceToNextDeal_AfterARoundSettles_StartsAFreshRound()
    {
        Room room = CreateStartedRoom();
        PlayOutRound(room);
        Assert.Equal(GamePhase.WaitingForDeal, room.Game!.GamePhase);

        bool advanced = room.AdvanceToNextDeal();

        Assert.True(advanced);
        Assert.Equal(RoomState.InProgress, room.State);
        Assert.Equal(GamePhase.MichiganRummyInProgress, room.Game.GamePhase);
        Assert.Null(room.Game.CurrentMichiganRound!.WinnerIndex);
    }

    [Fact]
    public void PlayingAFullGame_EndsCompletedWithACleanFinalProjection()
    {
        const int seed = 1;
        Room room = CreateStartedRoom(new Random(seed));

        int safety = 0;
        while (room.State == RoomState.InProgress && safety++ < 100)
        {
            PlayOutRound(room);
            room.AdvanceToNextDeal();
        }

        Assert.True(room.State == RoomState.Completed, $"Seed {seed}: game never completed within {safety} deals.");
        Assert.Equal(GamePhase.GameComplete, room.Game!.GamePhase);
        Assert.NotNull(room.Game.CurrentMichiganRound);

        GameStateView view = GameStateProjector.ToGameStateView(room.Game);
        Assert.Equal(GamePhase.GameComplete, view.Phase);
        Assert.Empty(view.Seats);
        Assert.Null(view.CurrentLeaderIndex);
        Assert.Null(view.LastPlayedCard);
        Assert.Null(view.WinnerIndex);
    }

    [Fact]
    public void NoWinnerRound_OnlyHappensWhenEveryHandHoldsOnlyTheStoppedSuit()
    {
        for (int seed = 0; seed < 40; seed++)
        {
            Room room = CreateStartedRoom(new Random(seed));
            while (room.State == RoomState.InProgress)
            {
                Game g = room.Game!;
                PlayOutRound(room);
                MichiganRound round = g.CurrentMichiganRound!;
                if (round.WinnerIndex is null && g.CurrentDeal is { } deal && round.LastPlayedCard is { } last)
                {
                    foreach (Seat seat in deal.Seats)
                        Assert.True(
                            seat.Hand.Cards.All(c => c.Suit == last.Suit),
                            $"Seed {seed}: draw while a seat still holds a non-{last.Suit} card: "
                                + string.Join(",", seat.Hand.Cards.Select(c => $"{c.Rank}{c.Suit}")));
                }
                room.AdvanceToNextDeal();
            }
        }
    }

    private static Room CreateStartedRoom() => CreateStartedRoom(random: null);

    private static Room CreateStartedRoom(Random? random)
    {
        Room room = new("ABCD", random);
        AddReadyMember(room, "user-1", "Jeff");
        AddReadyMember(room, "user-2", "Ann");
        room.StartGame();
        return room;
    }

    private static void AddReadyMember(Room room, string userId, string name)
    {
        room.AddMember(userId, name);
        room.SetReady(userId, true);
    }

    private static void PlayOutRound(Room room)
    {
        Game game = room.Game!;
        while (game.GamePhase == GamePhase.MichiganRummyInProgress)
        {
            (string userId, Card card) = NextLegalMove(room, game);
            Assert.True(room.TryPlayCard(userId, card));
        }
    }

    private static (string UserId, Card Card) NextLegalMove(Room room, Game game)
    {
        MichiganRound round = game.CurrentMichiganRound!;
        IReadOnlyList<Seat> seats = game.CurrentDeal!.Seats;

        if (round.Phase == RoundPhase.RunInProgress && round.LastPlayedCard is { } last && last.Rank != Rank.Ace)
        {
            Card next = new(last.Suit, last.Rank + 1);
            Seat holder = seats.First(seat => seat.Hand.Cards.Contains(next));
            return (UserIdOf(room, holder), next);
        }

        Seat leader = seats[round.CurrentLeaderIndex];
        IEnumerable<Card> candidates = round.Phase == RoundPhase.RunStopped && round.LastPlayedCard is { } stop
            ? leader.Hand.Cards.Where(card => card.Suit != stop.Suit)
            : leader.Hand.Cards;
        Card lead = candidates.OrderBy(card => card.Suit).ThenBy(card => card.Rank).First();
        return (UserIdOf(room, leader), lead);
    }

    private static string UserIdOf(Room room, Seat seat) =>
        room.Members().First(member => member.Player.Id == seat.Player.Id).UserId;
}
