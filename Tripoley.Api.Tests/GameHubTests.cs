using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using Tripoley.Api.Contracts;
using Tripoley.Api.Hubs;
using Tripoley.Api.Rooms;
using Tripoley.Engine;

namespace Tripoley.Api.Tests;

public class GameHubTests
{
    private static GameHub BuildHub(
        IRoomRegistry registry,
        string connectionId,
        out IHubCallerClients clients,
        out IGroupManager groups)
    {
        var caller = Substitute.For<ISingleClientProxy>();
        var clientProxies = new Dictionary<string, ISingleClientProxy>();
        var groupProxies = new Dictionary<string, IClientProxy>();

        clients = Substitute.For<IHubCallerClients>();
        clients.Caller.Returns(caller);
        clients.Client(Arg.Any<string>())
            .Returns(call => Cached(clientProxies, (string)call[0]!, () => Substitute.For<ISingleClientProxy>()));
        clients.Group(Arg.Any<string>())
            .Returns(call => Cached(groupProxies, (string)call[0]!, () => Substitute.For<IClientProxy>()));

        groups = Substitute.For<IGroupManager>();

        var context = Substitute.For<HubCallerContext>();
        context.ConnectionId.Returns(connectionId);

        return new GameHub(registry) { Clients = clients, Groups = groups, Context = context };
    }

    private static TProxy Cached<TProxy>(Dictionary<string, TProxy> cache, string key, Func<TProxy> factory)
    {
        if (!cache.TryGetValue(key, out TProxy? proxy))
        {
            proxy = factory();
            cache[key] = proxy;
        }
        return proxy;
    }

    private static T LastMessage<T>(IClientProxy proxy, string eventName)
    {
        object?[] payload = proxy.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(IClientProxy.SendCoreAsync))
            .Select(call => call.GetArguments())
            .Where(args => (string)args[0]! == eventName)
            .Select(args => (object?[])args[1]!)
            .Last();
        return (T)payload[0]!;
    }

    private const string LobbyGroup = "__lobby__";

    private static Room StartedRoom(IRoomRegistry registry)
    {
        var room = registry.Create();
        room.AddMember("c0", "Alice");
        room.AddMember("c1", "Bob");
        room.SetReady("c0", true);
        room.SetReady("c1", true);
        Assert.True(room.StartGame());
        return room;
    }

    [Fact]
    public async Task CreateRoom_WithBlankName_Throws()
    {
        var hub = BuildHub(new InMemoryRoomRegistry(), "c1", out _, out _);
        await Assert.ThrowsAsync<HubException>(() => hub.CreateRoom("   "));
    }

    [Fact]
    public async Task CreateRoom_WithAName_RegistersTheRoomAndJoinsItsGroup()
    {
        var registry = new InMemoryRoomRegistry();
        var hub = BuildHub(registry, "c1", out _, out var groups);

        RoomView view = await hub.CreateRoom("Jeff");

        Assert.False(string.IsNullOrWhiteSpace(view.Code));
        Room? room = registry.Find(view.Code);
        Assert.NotNull(room);
        Assert.NotNull(room!.FindMember("c1"));
        await groups.Received().AddToGroupAsync("c1", view.Code, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task JoinRoom_WithBlankCode_Throws()
    {
        var hub = BuildHub(new InMemoryRoomRegistry(), "c1", out _, out _);
        await Assert.ThrowsAsync<HubException>(() => hub.JoinRoom("   ", "Jeff"));
    }

    [Fact]
    public async Task JoinRoom_WithUnknownCode_Throws()
    {
        var hub = BuildHub(new InMemoryRoomRegistry(), "c1", out _, out _);
        await Assert.ThrowsAsync<HubException>(() => hub.JoinRoom("ZZZZZZ", "Jeff"));
    }

    [Fact]
    public async Task JoinRoom_WithAnOpenRoom_AddsTheMember()
    {
        var registry = new InMemoryRoomRegistry();
        var room = registry.Create();
        room.AddMember("c0", "Alice");
        var hub = BuildHub(registry, "c1", out _, out _);

        RoomView view = await hub.JoinRoom(room.Code, "Bob");

        Assert.Equal(room.Code, view.Code);
        Assert.NotNull(room.FindMember("c1"));
    }

    [Fact]
    public async Task SetReady_InALobby_MarksTheMemberReady()
    {
        var registry = new InMemoryRoomRegistry();
        var room = registry.Create();
        room.AddMember("c1", "Alice");
        var hub = BuildHub(registry, "c1", out _, out _);

        await hub.SetReady(true);

        Assert.True(room.FindMember("c1")!.IsReady);
    }

    [Fact]
    public async Task JoinRoom_WhenTheGameHasStarted_Throws()
    {
        var registry = new InMemoryRoomRegistry();
        Room room = StartedRoom(registry);
        var hub = BuildHub(registry, "late", out _, out _);

        await Assert.ThrowsAsync<HubException>(() => hub.JoinRoom(room.Code, "Latecomer"));
    }

    [Fact]
    public async Task SetReady_WhenNotInARoom_Throws()
    {
        var hub = BuildHub(new InMemoryRoomRegistry(), "nobody", out _, out _);
        await Assert.ThrowsAsync<HubException>(() => hub.SetReady(true));
    }

    [Fact]
    public async Task StartGame_WhenNotInARoom_Throws()
    {
        var hub = BuildHub(new InMemoryRoomRegistry(), "nobody", out _, out _);
        await Assert.ThrowsAsync<HubException>(() => hub.StartGame());
    }

    [Fact]
    public async Task PlayCard_WhenNotInARoom_Throws()
    {
        var hub = BuildHub(new InMemoryRoomRegistry(), "nobody", out _, out _);
        await Assert.ThrowsAsync<HubException>(() => hub.PlayCard(new CardDto(Suit.Spades, Rank.Two)));
    }

    [Fact]
    public async Task StartGame_WithoutEnoughReadyPlayers_Throws()
    {
        var registry = new InMemoryRoomRegistry();
        var room = registry.Create();
        room.AddMember("c0", "Alice");
        var hub = BuildHub(registry, "c0", out _, out _);

        await Assert.ThrowsAsync<HubException>(() => hub.StartGame());
    }

    [Fact]
    public async Task StartGame_WithReadyPlayers_StartsTheGame()
    {
        var registry = new InMemoryRoomRegistry();
        var room = registry.Create();
        room.AddMember("c0", "Alice");
        room.AddMember("c1", "Bob");
        room.SetReady("c0", true);
        room.SetReady("c1", true);
        var hub = BuildHub(registry, "c0", out _, out _);

        await hub.StartGame();

        Assert.Equal(RoomState.InProgress, room.State);
    }

    [Fact]
    public async Task PlayCard_WithAnIllegalMove_Throws()
    {
        var registry = new InMemoryRoomRegistry();
        Room room = StartedRoom(registry);
        var hub = BuildHub(registry, "c0", out _, out _);

        await Assert.ThrowsAsync<HubException>(() => hub.PlayCard(new CardDto(Suit.Spades, Rank.Two)));
    }

    [Fact]
    public async Task PlayCard_WithALegalLead_IsAccepted()
    {
        var registry = new InMemoryRoomRegistry();
        Room room = StartedRoom(registry);
        Game game = room.Game!;
        int leaderIndex = game.CurrentMichiganRound!.CurrentLeaderIndex;
        Seat leaderSeat = game.CurrentDeal!.Seats[leaderIndex];
        Card lead = leaderSeat.Hand.Cards.First(card => RunEngine.CanLeadRun(leaderSeat.Hand, card));
        RoomMember leader = room.Members().First(member => member.Player.Id == leaderSeat.Player.Id);
        var hub = BuildHub(registry, leader.ConnectionId, out _, out _);

        await hub.PlayCard(new CardDto(lead.Suit, lead.Rank));

        Assert.DoesNotContain(lead, leaderSeat.Hand.Cards);
    }

    [Fact]
    public async Task OnConnectedAsync_PutsTheCallerInTheLobbyGroup()
    {
        var hub = BuildHub(new InMemoryRoomRegistry(), "c1", out _, out var groups);

        await hub.OnConnectedAsync();

        await groups.Received().AddToGroupAsync("c1", Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task OnDisconnectedAsync_RemovesTheMemberAndEmptiesTheRoom()
    {
        var registry = new InMemoryRoomRegistry();
        var room = registry.Create();
        room.AddMember("c1", "Alice");
        var hub = BuildHub(registry, "c1", out _, out _);

        await hub.OnDisconnectedAsync(null);

        Assert.Null(room.FindMember("c1"));
        Assert.Null(registry.Find(room.Code));
    }

    [Fact]
    public async Task PlayCard_SendsEachMemberOnlyTheirOwnHand()
    {
        var registry = new InMemoryRoomRegistry();
        Room room = StartedRoom(registry);
        Game game = room.Game!;
        int leaderIndex = game.CurrentMichiganRound!.CurrentLeaderIndex;
        Seat leaderSeat = game.CurrentDeal!.Seats[leaderIndex];
        Card lead = leaderSeat.Hand.Cards.First(card => RunEngine.CanLeadRun(leaderSeat.Hand, card));
        RoomMember leader = room.Members().First(member => member.Player.Id == leaderSeat.Player.Id);
        var hub = BuildHub(registry, leader.ConnectionId, out var clients, out _);

        await hub.PlayCard(new CardDto(lead.Suit, lead.Rank));

        foreach (RoomMember member in room.Members())
        {
            Seat seat = game.CurrentDeal!.Seats.First(s => s.Player.Id == member.Player.Id);
            var expected = seat.Hand.Cards.Select(card => new CardDto(card.Suit, card.Rank)).ToList();
            SelfView self = LastMessage<SelfView>(clients.Client(member.ConnectionId), ClientEvents.HandUpdated);
            Assert.Equal(expected, self.Hand);
        }
    }

    [Fact]
    public async Task PlayCard_GivesDifferentMembersDifferentHands()
    {
        var registry = new InMemoryRoomRegistry();
        Room room = StartedRoom(registry);
        Game game = room.Game!;
        int leaderIndex = game.CurrentMichiganRound!.CurrentLeaderIndex;
        Seat leaderSeat = game.CurrentDeal!.Seats[leaderIndex];
        Card lead = leaderSeat.Hand.Cards.First(card => RunEngine.CanLeadRun(leaderSeat.Hand, card));
        RoomMember leader = room.Members().First(member => member.Player.Id == leaderSeat.Player.Id);
        var hub = BuildHub(registry, leader.ConnectionId, out var clients, out _);

        await hub.PlayCard(new CardDto(lead.Suit, lead.Rank));

        SelfView alice = LastMessage<SelfView>(clients.Client("c0"), ClientEvents.HandUpdated);
        SelfView bob = LastMessage<SelfView>(clients.Client("c1"), ClientEvents.HandUpdated);
        Assert.NotEqual(alice.Hand, bob.Hand);
    }

    [Fact]
    public async Task OnConnectedAsync_SendsTheCallerTheOpenRoomsSnapshot()
    {
        var hub = BuildHub(new InMemoryRoomRegistry(), "c1", out var clients, out _);

        await hub.OnConnectedAsync();

        var rooms = LastMessage<IReadOnlyList<OpenRoomView>>(clients.Caller, ClientEvents.OpenRoomsUpdated);
        Assert.Empty(rooms);
    }

    [Fact]
    public async Task CreateRoom_BroadcastsTheOpenRoomToTheLobby()
    {
        var registry = new InMemoryRoomRegistry();
        var hub = BuildHub(registry, "c1", out var clients, out _);

        RoomView view = await hub.CreateRoom("Jeff");

        var rooms = LastMessage<IReadOnlyList<OpenRoomView>>(clients.Group(LobbyGroup), ClientEvents.OpenRoomsUpdated);
        Assert.Contains(rooms, room => room.Code == view.Code);
    }

    [Fact]
    public async Task JoinRoom_BroadcastsTheUpdatedLobbyToTheRoom()
    {
        var registry = new InMemoryRoomRegistry();
        var room = registry.Create();
        room.AddMember("c0", "Alice");
        var hub = BuildHub(registry, "c1", out var clients, out _);

        await hub.JoinRoom(room.Code, "Bob");

        RoomView lobby = LastMessage<RoomView>(clients.Group(room.Code), ClientEvents.LobbyUpdated);
        Assert.Contains(lobby.Members, member => member.Name == "Bob");
    }

    [Fact]
    public async Task StartGame_BroadcastsGameStateToTheRoomAndHandsToEachMember()
    {
        var registry = new InMemoryRoomRegistry();
        var room = registry.Create();
        room.AddMember("c0", "Alice");
        room.AddMember("c1", "Bob");
        room.SetReady("c0", true);
        room.SetReady("c1", true);
        var hub = BuildHub(registry, "c0", out var clients, out _);

        await hub.StartGame();

        GameStateView state = LastMessage<GameStateView>(clients.Group(room.Code), ClientEvents.GameStateUpdated);
        Assert.Equal(GamePhase.MichiganRummyInProgress, state.Phase);
        SelfView alice = LastMessage<SelfView>(clients.Client("c0"), ClientEvents.HandUpdated);
        SelfView bob = LastMessage<SelfView>(clients.Client("c1"), ClientEvents.HandUpdated);
        Assert.NotEmpty(alice.Hand);
        Assert.NotEmpty(bob.Hand);
    }

    [Fact]
    public async Task OnDisconnectedAsync_WhenOtherMembersRemain_KeepsTheRoomAndBroadcastsLobby()
    {
        var registry = new InMemoryRoomRegistry();
        var room = registry.Create();
        room.AddMember("c0", "Alice");
        room.AddMember("c1", "Bob");
        var hub = BuildHub(registry, "c1", out var clients, out _);

        await hub.OnDisconnectedAsync(null);

        Assert.Null(room.FindMember("c1"));
        Assert.NotNull(room.FindMember("c0"));
        Assert.Same(room, registry.Find(room.Code));
        RoomView lobby = LastMessage<RoomView>(clients.Group(room.Code), ClientEvents.LobbyUpdated);
        Assert.DoesNotContain(lobby.Members, member => member.Name == "Bob");
    }
}
