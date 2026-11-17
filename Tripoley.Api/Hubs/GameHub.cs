namespace Tripoley.Api.Hubs;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Tripoley.Api.Rooms;
using Tripoley.Api.Contracts;
using Tripoley.Api.Projections;
using Tripoley.Engine;

[Authorize]
public sealed class GameHub(IRoomRegistry roomRegistry, ILogger<GameHub> logger) : Hub
{
    private const string LobbyGroup = "__lobby__";

    private string UserId => Context.UserIdentifier ?? throw new HubException("Not authenticated.");

    public override async Task OnConnectedAsync()
    {
        var room = roomRegistry.FindByUserId(UserId);
        if (room is not null)
        {
            room.MarkConnected(UserId);
            await Groups.AddToGroupAsync(Context.ConnectionId, room.Code);
            logger.LogInformation("User {UserId} reconnected to room {RoomCode}.", UserId, room.Code);

            if (room.Game is not null)
            {
                await Clients.Caller.SendAsync(ClientEvents.LobbyUpdated, RoomView.FromRoom(room));
                await BroadcastGameState(room);
            }
            else
            {
                await BroadcastLobby(room);
            }
        }
        else
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, LobbyGroup);
            await Clients.Caller.SendAsync(ClientEvents.OpenRoomsUpdated, BuildOpenRooms());
        }

        await base.OnConnectedAsync();
    }

    public async Task<RoomView> CreateRoom(string playerName)
    {
        var name = RequireName(playerName);
        RequireNotAlreadyInARoom();

        var room = roomRegistry.Create();
        room.AddMember(UserId, name);
        await Groups.AddToGroupAsync(Context.ConnectionId, room.Code);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, LobbyGroup);
        logger.LogInformation("User {UserId} created room {RoomCode}.", UserId, room.Code);
        await BroadcastOpenRooms();
        return RoomView.FromRoom(room);
    }

    public async Task<RoomView> JoinRoom(string roomCode, string playerName)
    {
        var name = RequireName(playerName);
        var code = RequireCode(roomCode);
        RequireNotAlreadyInARoom();

        var room = roomRegistry.Find(code)
            ?? throw new HubException("No room with that code.");

        if (room.AddMember(UserId, name) is null)
            throw new HubException("Could not join: the room is full or the game has already started.");

        await Groups.AddToGroupAsync(Context.ConnectionId, room.Code);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, LobbyGroup);
        logger.LogInformation("User {UserId} joined room {RoomCode}.", UserId, room.Code);
        await BroadcastLobby(room);
        await BroadcastOpenRooms();
        return RoomView.FromRoom(room);
    }

    public async Task SetReady(bool isReady)
    {
        var room = GetRoomOrThrow();
        room.SetReady(UserId, isReady);
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
        if (!room.TryPlayCard(UserId, new Card(card.Suit, card.Rank)))
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
        var room = roomRegistry.FindByUserId(UserId);
        if (room is not null)
        {
            room.MarkDisconnected(UserId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, room.Code);
            logger.LogInformation("User {UserId} disconnected from room {RoomCode}; seat kept for reconnect.", UserId, room.Code);

            if (room.Game is not null)
                await BroadcastGameState(room);
            else
                await BroadcastLobby(room);
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

    private void RequireNotAlreadyInARoom()
    {
        if (roomRegistry.FindByUserId(UserId) is not null)
            throw new HubException("You are already in a room.");
    }

    private Room GetRoomOrThrow() =>
        roomRegistry.FindByUserId(UserId)
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
        var (state, hands) = room.ReadGame((game, lastWinner, members) =>
        {
            var connected = members.ToDictionary(member => member.Player.Id, member => member.IsConnected);
            var stateView = GameStateProjector.ToGameStateView(game, lastWinner, connected);
            var handViews = members
                .Select(member => (member.UserId, View: GameStateProjector.ToSelfView(game, member.Player.Id)))
                .ToList();
            return (stateView, handViews);
        });

        await Clients.Group(room.Code).SendAsync(ClientEvents.GameStateUpdated, state);

        foreach (var (userId, view) in hands)
            await Clients.User(userId).SendAsync(ClientEvents.HandUpdated, view);
    }
}
