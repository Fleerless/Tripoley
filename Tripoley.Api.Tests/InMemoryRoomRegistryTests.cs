using System.Collections.Concurrent;
using Microsoft.Extensions.Time.Testing;
using Tripoley.Api.Rooms;

namespace Tripoley.Api.Tests;

public class InMemoryRoomRegistryTests
{
    [Fact]
    public void Create_ReturnsRoomWithNonEmptyCodeInLobby()
    {
        var registry = new InMemoryRoomRegistry();

        Room room = registry.Create();

        Assert.False(string.IsNullOrWhiteSpace(room.Code));
        Assert.Equal(RoomState.Lobby, room.State);
    }

    [Fact]
    public void Create_ProducesDistinctCodes()
    {
        var registry = new InMemoryRoomRegistry();

        List<string> codes = Enumerable.Range(0, 100)
            .Select(_ => registry.Create().Code)
            .ToList();

        Assert.Equal(codes.Count, codes.Distinct().Count());
    }

    [Fact]
    public void Find_ReturnsTheCreatedRoom()
    {
        var registry = new InMemoryRoomRegistry();
        Room created = registry.Create();

        Room? found = registry.Find(created.Code);

        Assert.Same(created, found);
    }

    [Fact]
    public void Find_UnknownCode_ReturnsNull()
    {
        var registry = new InMemoryRoomRegistry();

        Assert.Null(registry.Find("NOPE"));
    }

    [Fact]
    public void FindByUserId_ReturnsRoomContainingTheUser()
    {
        var registry = new InMemoryRoomRegistry();
        Room room = registry.Create();
        room.AddMember("user-1", "Jeff");

        Room? found = registry.FindByUserId("user-1");

        Assert.Same(room, found);
    }

    [Fact]
    public void FindByUserId_UnknownUser_ReturnsNull()
    {
        var registry = new InMemoryRoomRegistry();
        Room room = registry.Create();
        room.AddMember("user-1", "Jeff");

        Assert.Null(registry.FindByUserId("user-unknown"));
    }

    [Fact]
    public void Remove_MakesSubsequentFindReturnNull()
    {
        var registry = new InMemoryRoomRegistry();
        Room room = registry.Create();

        registry.Remove(room.Code);

        Assert.Null(registry.Find(room.Code));
    }

    [Fact]
    public void Remove_AlsoDropsTheUserFromTheByUserIndex()
    {
        var registry = new InMemoryRoomRegistry();
        Room room = registry.Create();
        room.AddMember("user-1", "Jeff");

        registry.Remove(room.Code);

        Assert.Null(registry.FindByUserId("user-1"));
    }

    [Fact]
    public void Remove_UnknownCode_DoesNotThrow()
    {
        var registry = new InMemoryRoomRegistry();

        registry.Remove("NOPE");
    }

    [Fact]
    public void RemoveIdle_DropsRoomsAbandonedBeforeTheCutoff()
    {
        var time = new FakeTimeProvider();
        var registry = new InMemoryRoomRegistry(time);
        Room room = registry.Create();
        room.AddMember("user-1", "Jeff");
        room.MarkDisconnected("user-1");

        time.Advance(TimeSpan.FromMinutes(31));
        int removed = registry.RemoveIdle(time.GetUtcNow() - TimeSpan.FromMinutes(30));

        Assert.Equal(1, removed);
        Assert.Null(registry.Find(room.Code));
    }

    [Fact]
    public void RemoveIdle_KeepsRoomsThatAreStillOccupiedOrRecentlyIdle()
    {
        var time = new FakeTimeProvider();
        var registry = new InMemoryRoomRegistry(time);

        Room occupied = registry.Create();
        occupied.AddMember("user-1", "Jeff");

        Room recentlyIdle = registry.Create();
        recentlyIdle.AddMember("user-2", "Ann");
        recentlyIdle.MarkDisconnected("user-2");

        time.Advance(TimeSpan.FromMinutes(5));
        int removed = registry.RemoveIdle(time.GetUtcNow() - TimeSpan.FromMinutes(30));

        Assert.Equal(0, removed);
        Assert.Same(occupied, registry.Find(occupied.Code));
        Assert.Same(recentlyIdle, registry.Find(recentlyIdle.Code));
    }

    [Fact]
    public void Create_CalledConcurrently_RegistersEveryRoomWithADistinctCode()
    {
        var registry = new InMemoryRoomRegistry();
        var rooms = new ConcurrentBag<Room>();

        Parallel.For(0, 1000, _ => rooms.Add(registry.Create()));

        string[] codes = rooms.Select(room => room.Code).ToArray();
        Assert.Equal(1000, codes.Length);
        Assert.Equal(codes.Length, codes.Distinct().Count());
    }
}
