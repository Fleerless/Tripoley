namespace Tripoley.Api.Hubs;

using Microsoft.AspNetCore.SignalR;
using Tripoley.Api.Rooms;
using Tripoley.Api.Contracts;
using Tripoley.Api.Projections;
using Tripoley.Engine;

public sealed class GameHub(IRoomRegistry roomRegistry) : Hub
{
    private const string LobbyGroup = "__lobby__";

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, LobbyGroup);
        await Clients.Caller.SendAsync(ClientEvents.OpenRoomsUpdated, BuildOpenRooms());
        await base.OnConnectedAsync();
    }

    public async Task<RoomView> CreateRoom(string playerName)
    {
        var name = RequireName(playerName);
        var room = roomRegistry.Create();
        room.AddMember(Context.ConnectionId, name);
        await Groups.AddToGroupAsync(Context.ConnectionId, room.Code);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, LobbyGroup);
        await BroadcastOpenRooms();
        return RoomView.FromRoom(room);
    }

    public async Task<RoomView> JoinRoom(string roomCode, string playerName)
    {
        var name = RequireName(playerName);
        var code = RequireCode(roomCode);

        var room = roomRegistry.Find(code)
            ?? throw new HubException("No room with that code.");

        if (room.AddMember(Context.ConnectionId, name) is null)
            throw new HubException("Could not join: the room is full or the game has already started.");

        await Groups.AddToGroupAsync(Context.ConnectionId, room.Code);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, LobbyGroup);
        await BroadcastLobby(room);
        await BroadcastOpenRooms();
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

        await BroadcastLobby(room);
        await BroadcastGameState(room);
        await BroadcastOpenRooms();
    }

    // This play functionality will need to be re-thought/expanded when new game phases are added
    public async Task PlayCard(CardDto card)
    {
        var room = GetRoomOrThrow();
        if (!room.TryPlayCard(Context.ConnectionId, new Card(card.Suit, card.Rank)))
            throw new HubException("That move isn't legal right now.");

        await BroadcastGameState(room);

        if (room.AdvanceToNextDeal())
        {
            await BroadcastGameState(room);
            if (room.State == RoomState.Completed)
                await BroadcastLobby(room);
        }
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

            await BroadcastOpenRooms();
        }

        await base.OnDisconnectedAsync(exception);
    }

    private static string RequireName(string playerName)
    {
        var name = (playerName ?? string.Empty).Trim();
        if (name.Length == 0) throw new HubException("Please enter a name.");
        return name;
    }

    private static string RequireCode(string roomCode)
    {
        var code = (roomCode ?? string.Empty).Trim().ToUpperInvariant();
        if (code.Length == 0) throw new HubException("Please enter a room code.");
        return code;
    }

    private Room GetRoomOrThrow() =>
        roomRegistry.FindByConnectionId(Context.ConnectionId)
            ?? throw new HubException("You are not in a room.");

    private Task BroadcastLobby(Room room) =>
        Clients.Group(room.Code).SendAsync(ClientEvents.LobbyUpdated, RoomView.FromRoom(room));

    private Task BroadcastOpenRooms() =>
        Clients.Group(LobbyGroup).SendAsync(ClientEvents.OpenRoomsUpdated, BuildOpenRooms());

    private IReadOnlyList<OpenRoomView> BuildOpenRooms() =>
        roomRegistry.OpenRooms()
            .Select(room => new OpenRoomView(room.Code, room.Members().Count, Game.MaxPlayerCount))
            .ToList();

    private async Task BroadcastGameState(Room room)
    {
        var state = GameStateProjector.ToGameStateView(room.Game!, room.LastRoundWinner);
        await Clients.Group(room.Code).SendAsync(ClientEvents.GameStateUpdated, state);

        foreach (var member in room.Members())
            await Clients.Client(member.ConnectionId)
                .SendAsync(ClientEvents.HandUpdated, GameStateProjector.ToSelfView(room.Game!, member.Player.Id));
    }
}
