using System.Diagnostics;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Tripoley.Api.Contracts;
using Tripoley.Api.Hubs;
using Tripoley.Api.Tests.Auth;
using Tripoley.Engine;

namespace Tripoley.Api.Tests;

public sealed class GameHubIntegrationTests : IDisposable
{
    private readonly TestWebAppFactory factory = new();

    public void Dispose() => factory.Dispose();

    [Fact]
    public async Task Hub_WithoutAToken_RejectsTheConnection()
    {
        HubConnection connection = new HubConnectionBuilder()
            .WithUrl("http://localhost/hubs/gamehub", options =>
            {
                options.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler();
                options.Transports = HttpTransportType.LongPolling;
            })
            .Build();

        await Assert.ThrowsAnyAsync<Exception>(() => connection.StartAsync());
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task TwoPlayers_StartAndPlayOverSignalR_EachKeepsAPrivateHand()
    {
        await using TestClient alice = await ConnectAsync();
        await using TestClient bob = await ConnectAsync();

        RoomView room = await alice.Connection.InvokeAsync<RoomView>("CreateRoom", "Alice");
        await bob.Connection.InvokeAsync<RoomView>("JoinRoom", room.Code, "Bob");
        await alice.Connection.InvokeAsync("SetReady", true);
        await bob.Connection.InvokeAsync("SetReady", true);
        await alice.Connection.InvokeAsync("StartGame");

        await WaitUntil(() => alice.LatestState?.Phase == GamePhase.MichiganRummyInProgress);
        GameStateView state = alice.LatestState!;
        Assert.Equal(2, state.Seats.Count);
        Assert.All(state.Seats, seat => Assert.True(seat.CardsRemaining > 0));

        await WaitUntil(() => alice.LatestHand is { Hand.Count: > 0 } && bob.LatestHand is { Hand.Count: > 0 });
        SelfView aliceHand = alice.LatestHand!;
        SelfView bobHand = bob.LatestHand!;
        Assert.NotEqual(aliceHand.Hand, bobHand.Hand);

        TestClient leader = aliceHand.LegalPlays.Count > 0 ? alice : bob;
        CardDto lead = (leader == alice ? aliceHand : bobHand).LegalPlays[0];
        await leader.Connection.InvokeAsync("PlayCard", lead);

        await WaitUntil(() => leader.LatestState?.LastPlayedCard is not null);
        Assert.Equal(lead, leader.LatestState!.LastPlayedCard);
    }

    [Fact]
    public async Task AfterADrop_ReconnectingWithTheSameIdentity_ReclaimsTheSeatAndPrivateHand()
    {
        string aliceId = Guid.NewGuid().ToString();
        await using TestClient bob = await ConnectAsync();
        TestClient alice = await ConnectAsync(aliceId);

        RoomView room = await alice.Connection.InvokeAsync<RoomView>("CreateRoom", "Alice");
        await bob.Connection.InvokeAsync("JoinRoom", room.Code, "Bob");
        await alice.Connection.InvokeAsync("SetReady", true);
        await bob.Connection.InvokeAsync("SetReady", true);
        await alice.Connection.InvokeAsync("StartGame");

        await WaitUntil(() => bob.LatestState?.Phase == GamePhase.MichiganRummyInProgress);

        await alice.Connection.DisposeAsync();
        await WaitUntil(() => bob.LatestState!.Seats.Single(seat => seat.Name == "Alice").IsConnected == false);

        await using TestClient reconnected = await ConnectAsync(aliceId);
        await WaitUntil(() => reconnected.LatestState?.Phase == GamePhase.MichiganRummyInProgress);
        await WaitUntil(() => reconnected.LatestHand is { Hand.Count: > 0 });

        await WaitUntil(() => bob.LatestState!.Seats.Single(seat => seat.Name == "Alice").IsConnected);
    }

    [Fact]
    public async Task InTheLobby_WhenAMemberDropsAndReconnects_PeersSeeThemOfflineThenBackOnline()
    {
        string bobId = Guid.NewGuid().ToString();
        await using TestClient alice = await ConnectAsync();
        TestClient bob = await ConnectAsync(bobId);

        RoomView room = await alice.Connection.InvokeAsync<RoomView>("CreateRoom", "Alice");
        await bob.Connection.InvokeAsync<RoomView>("JoinRoom", room.Code, "Bob");
        await WaitUntil(() => alice.LatestLobby?.Members.Any(member => member.Name == "Bob") == true);

        await bob.Connection.DisposeAsync();
        await WaitUntil(() => alice.LatestLobby!.Members.Single(member => member.Name == "Bob").IsConnected == false);

        await using TestClient reconnected = await ConnectAsync(bobId);
        await WaitUntil(() => alice.LatestLobby!.Members.Single(member => member.Name == "Bob").IsConnected);
    }

    [Fact]
    public async Task OnConnect_TheClientJoinsTheLobbyGroupAndReceivesOpenRoomUpdates()
    {
        await using TestClient watcher = await ConnectAsync();
        await WaitUntil(() => watcher.LatestOpenRooms is not null);
        Assert.Empty(watcher.LatestOpenRooms!);

        await using TestClient host = await ConnectAsync();
        RoomView room = await host.Connection.InvokeAsync<RoomView>("CreateRoom", "Host");

        await WaitUntil(() => watcher.LatestOpenRooms!.Any(open => open.Code == room.Code));
    }

    private async Task<TestClient> ConnectAsync(string? userId = null)
    {
        string token = TestTokens.Mint(userId ?? Guid.NewGuid().ToString());
        HubConnection connection = new HubConnectionBuilder()
            .WithUrl("http://localhost/hubs/gamehub", options =>
            {
                options.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler();
                options.Transports = HttpTransportType.LongPolling;
                options.AccessTokenProvider = () => Task.FromResult<string?>(token);
            })
            .AddJsonProtocol(options =>
                options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
            .Build();

        TestClient client = new(connection);
        connection.On<GameStateView>(ClientEvents.GameStateUpdated, state => client.LatestState = state);
        connection.On<SelfView>(ClientEvents.HandUpdated, hand => client.LatestHand = hand);
        connection.On<RoomView>(ClientEvents.LobbyUpdated, lobby => client.LatestLobby = lobby);
        connection.On<OpenRoomView[]>(ClientEvents.OpenRoomsUpdated, rooms => client.LatestOpenRooms = rooms);
        await connection.StartAsync();
        return client;
    }

    private static async Task WaitUntil(Func<bool> condition, int timeoutMs = 5000)
    {
        Stopwatch elapsed = Stopwatch.StartNew();
        while (!condition())
        {
            if (elapsed.ElapsedMilliseconds > timeoutMs)
                throw new TimeoutException("The expected SignalR message did not arrive in time.");
            await Task.Delay(25);
        }
    }

    private sealed class TestClient(HubConnection connection) : IAsyncDisposable
    {
        public HubConnection Connection { get; } = connection;
        public volatile GameStateView? LatestState;
        public volatile SelfView? LatestHand;
        public volatile RoomView? LatestLobby;
        public volatile OpenRoomView[]? LatestOpenRooms;

        public ValueTask DisposeAsync() => Connection.DisposeAsync();
    }
}
