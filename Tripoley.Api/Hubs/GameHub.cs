namespace Tripoley.Api.Hubs;

using Microsoft.AspNetCore.SignalR;
using Tripoley.Api.Rooms;
using Tripoley.Api.Contracts;
using Tripoley.Api.Projections;
using Tripoley.Engine;

public sealed class GameHub(IRoomRegistry roomRegistry) : Hub
{
    public async Task<RoomView> CreateRoom(string playerName)
    {
        var room = roomRegistry.Create();
        room.AddMember(Context.ConnectionId, playerName);
        await Groups.AddToGroupAsync(Context.ConnectionId, room.Code);
        return RoomView.FromRoom(room);
    }

    public async Task<RoomView> JoinRoom(string roomCode, string playerName)
    {
        var room = roomRegistry.Find(roomCode)
            ?? throw new HubException("No room with that code.");

        if (room.AddMember(Context.ConnectionId, playerName) is null)
            throw new HubException("Could not join: the room is full or the game has already started.");

        await Groups.AddToGroupAsync(Context.ConnectionId, room.Code);
        await BroadcastLobby(room);
        return RoomView.FromRoom(room);
    }

    public async Task SetReady(bool isReady)
    {
        var room = GetRoomOrThrow();
        room.SetReady(Context.ConnectionId, isReady);
        await BroadcastLobby(room);
    }

    public async Task StartGame()
    {
        var room = GetRoomOrThrow();
        if (!room.StartGame())
            throw new HubException("Can't start: need at least two players, all marked ready.");

        await BroadcastGameState(room);
    }

    public async Task PlayCard(CardDto card)
    {
        var room = GetRoomOrThrow();
        if (!room.TryPlayCard(Context.ConnectionId, new Card(card.Suit, card.Rank)))
            throw new HubException("That move isn't legal right now.");

        await BroadcastGameState(room);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var room = roomRegistry.FindByConnectionId(Context.ConnectionId);
        if (room is not null)
        {
            // TODO: mid-game disconnect. RemoveMember only acts in the lobby, engine can't drop a player yet.
            room.RemoveMember(Context.ConnectionId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, room.Code);

            if (room.Members().Count == 0)
                roomRegistry.Remove(room.Code);
            else
                await BroadcastLobby(room);
        }

        await base.OnDisconnectedAsync(exception);
    }

    private Room GetRoomOrThrow() =>
        roomRegistry.FindByConnectionId(Context.ConnectionId)
            ?? throw new HubException("You are not in a room.");

    private Task BroadcastLobby(Room room) =>
        Clients.Group(room.Code).SendAsync(ClientEvents.LobbyUpdated, RoomView.FromRoom(room));

    private async Task BroadcastGameState(Room room)
    {
        var state = GameStateProjector.ToGameStateView(room.Game!);
        await Clients.Group(room.Code).SendAsync(ClientEvents.GameStateUpdated, state);

        foreach (var member in room.Members())
            await Clients.Client(member.ConnectionId)
                .SendAsync(ClientEvents.HandUpdated, GameStateProjector.ToSelfView(room.Game!, member.Player.Id));
    }
}
