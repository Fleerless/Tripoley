using System.Diagnostics;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Tripoley.Api.Contracts;
using Tripoley.Api.Hubs;
using Tripoley.Engine;

namespace Tripoley.Api.Tests;

public class GameHubIntegrationTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
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

    private async Task<TestClient> ConnectAsync()
    {
        HubConnection connection = new HubConnectionBuilder()
            .WithUrl("http://localhost/hubs/gamehub", options =>
            {
                options.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler();
                options.Transports = HttpTransportType.LongPolling;
            })
            .AddJsonProtocol(options =>
                options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
            .Build();

        TestClient client = new(connection);
        connection.On<GameStateView>(ClientEvents.GameStateUpdated, state => client.LatestState = state);
        connection.On<SelfView>(ClientEvents.HandUpdated, hand => client.LatestHand = hand);
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

        public ValueTask DisposeAsync() => Connection.DisposeAsync();
    }
}
