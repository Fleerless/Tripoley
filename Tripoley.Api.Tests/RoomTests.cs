using Tripoley.Api.Rooms;
using Tripoley.Engine;

namespace Tripoley.Api.Tests;

public class RoomTests
{
    [Fact]
    public void AddMember_InLobby_AddsAndReturnsTheStoredMember()
    {
        Room room = new("ABCD");

        RoomMember? member = room.AddMember(connectionId: "conn-1", name: "Jeff");

        Assert.NotNull(member);
        Assert.Equal("conn-1", member.ConnectionId);
        Assert.Equal("Jeff", member.Player.Name);
        Assert.False(member.IsReady);
        // FindMember must return the same instance, not a freshly built copy.
        Assert.Same(member, room.FindMember("conn-1"));
    }

    [Fact]
    public void AddMember_GivesEachPlayerTheStartingChips()
    {
        Room room = new("ABCD");

        RoomMember member = room.AddMember("conn-1", "Jeff")!;

        Assert.Equal(Game.StartingChips, member.Player.ChipCount);
    }

    [Fact]
    public void AddMember_BeyondMaxPlayerCount_ReturnsNull()
    {
        Room room = new("ABCD");
        for (int seat = 0; seat < Game.MaxPlayerCount; seat++)
            Assert.NotNull(room.AddMember($"conn-{seat}", $"Player {seat}"));

        RoomMember? overflow = room.AddMember("conn-extra", "Latecomer");

        Assert.Null(overflow);
    }

    [Fact]
    public void AddMember_WhenNotInLobby_ReturnsNull()
    {
        Room room = CreateStartedRoom();

        Assert.Null(room.AddMember("conn-late", "Latecomer"));
    }

    [Fact]
    public void RemoveMember_InLobby_RemovesTheMember()
    {
        Room room = new("ABCD");
        room.AddMember("conn-1", "Jeff");

        bool removed = room.RemoveMember("conn-1");

        Assert.True(removed);
        Assert.Null(room.FindMember("conn-1"));
    }

    [Fact]
    public void RemoveMember_UnknownConnection_ReturnsFalse()
    {
        Room room = new("ABCD");
        room.AddMember("conn-1", "Jeff");

        Assert.False(room.RemoveMember("conn-unknown"));
    }

    [Fact]
    public void SetReady_TogglesTheStoredMember()
    {
        Room room = new("ABCD");
        room.AddMember("conn-1", "Jeff");

        bool set = room.SetReady("conn-1", true);

        Assert.True(set);
        Assert.True(room.FindMember("conn-1")!.IsReady);
    }

    [Fact]
    public void SetReady_UnknownConnection_ReturnsFalse()
    {
        Room room = new("ABCD");

        Assert.False(room.SetReady("conn-unknown", true));
    }

    [Fact]
    public void StartGame_WhenAllReadyAndEnoughPlayers_BeginsTheGame()
    {
        Room room = new("ABCD");
        AddReadyMember(room, "conn-1", "Jeff");
        AddReadyMember(room, "conn-2", "Ann");

        bool started = room.StartGame();

        Assert.True(started);
        Assert.Equal(RoomState.InProgress, room.State);
        Assert.NotNull(room.Game);
    }

    [Fact]
    public void StartGame_WithFewerThanMinPlayers_ReturnsFalseAndStaysInLobby()
    {
        Room room = new("ABCD");
        AddReadyMember(room, "conn-1", "Jeff");

        bool started = room.StartGame();

        Assert.False(started);
        Assert.Equal(RoomState.Lobby, room.State);
        Assert.Null(room.Game);
    }

    [Fact]
    public void StartGame_WhenNotEveryoneIsReady_ReturnsFalseAndStaysInLobby()
    {
        Room room = new("ABCD");
        AddReadyMember(room, "conn-1", "Jeff");
        room.AddMember("conn-2", "Ann");   // joined but not ready

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

    private static Room CreateStartedRoom()
    {
        Room room = new("ABCD");
        AddReadyMember(room, "conn-1", "Jeff");
        AddReadyMember(room, "conn-2", "Ann");
        room.StartGame();
        return room;
    }

    private static void AddReadyMember(Room room, string connectionId, string name)
    {
        room.AddMember(connectionId, name);
        room.SetReady(connectionId, true);
    }
}
