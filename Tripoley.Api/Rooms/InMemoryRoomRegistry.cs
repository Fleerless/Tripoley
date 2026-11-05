namespace Tripoley.Api.Rooms;
using System.Collections.Concurrent;
using System.Security.Cryptography;


public sealed class InMemoryRoomRegistry : IRoomRegistry
{
    private readonly ConcurrentDictionary<string, Room> _rooms = new();

    public Room Create()
    {        
        while (true)
        {
            string code = GenerateCode();
            var room = new Room(code);
            if (_rooms.TryAdd(code, room)) return room;  
        }
    }
    public Room? Find(string code)
    {
        if (_rooms.TryGetValue(code, out var room))
            return room;

        return null;
    }
    public Room? FindByConnectionId(string connectionId)
    {
        return _rooms.Values.FirstOrDefault(room => room.FindMember(connectionId) is not null);
    }
    public IReadOnlyList<Room> OpenRooms()
    {
        return _rooms.Values.Where(room => room.State == RoomState.Lobby).ToList();
    }
    public void Remove(string code)
    {
        _rooms.TryRemove(code, out _);
    }
    private static string GenerateCode()
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var code = new char[6];

    for (int index = 0; index < code.Length; index++)
        code[index] = alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];

    return new string(code);
}
}