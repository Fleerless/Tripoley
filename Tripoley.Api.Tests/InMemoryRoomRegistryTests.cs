using System.Collections.Concurrent;
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
    public void FindByConnectionId_ReturnsRoomContainingTheConnection()
    {
        var registry = new InMemoryRoomRegistry();
        Room room = registry.Create();
        room.AddMember("conn-1", "Jeff");

        Room? found = registry.FindByConnectionId("conn-1");

        Assert.Same(room, found);
    }

    [Fact]
    public void FindByConnectionId_UnknownConnection_ReturnsNull()
    {
        var registry = new InMemoryRoomRegistry();
        Room room = registry.Create();
        room.AddMember("conn-1", "Jeff");

        Assert.Null(registry.FindByConnectionId("conn-unknown"));
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
    public void Remove_UnknownCode_DoesNotThrow()
    {
        var registry = new InMemoryRoomRegistry();

        registry.Remove("NOPE");   // no-op, must not throw
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
