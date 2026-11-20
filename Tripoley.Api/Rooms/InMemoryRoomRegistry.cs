namespace Tripoley.Api.Rooms;
using System.Collections.Concurrent;
using System.Security.Cryptography;


public sealed class InMemoryRoomRegistry(TimeProvider? timeProvider = null) : IRoomRegistry
{
    private readonly ConcurrentDictionary<string, Room> _rooms = new();
    private readonly ConcurrentDictionary<string, Room> _roomByUser = new();
    private readonly TimeProvider _time = timeProvider ?? TimeProvider.System;

    public Room Create()
    {
        while (true)
        {
            string code = GenerateCode();
            var room = new Room(code, timeProvider: _time);
            room.MemberAdded += userId => _roomByUser[userId] = room;
            if (_rooms.TryAdd(code, room)) return room;
        }
    }
    public Room? Find(string code)
    {
        if (_rooms.TryGetValue(code, out var room))
            return room;

        return null;
    }
    public Room? FindByUserId(string userId)
    {
        return _roomByUser.TryGetValue(userId, out var room) ? room : null;
    }
    public Room? Leave(string userId)
    {
        if (!_roomByUser.TryGetValue(userId, out var room)) return null;

        room.Leave(userId);

        _roomByUser.TryRemove(new KeyValuePair<string, Room>(userId, room));
        if (room.Members().Count == 0)
            Remove(room.Code);
        return room;
    }
    public IReadOnlyList<Room> OpenRooms()
    {
        return _rooms.Values.Where(room => room.State == RoomState.Lobby).ToList();
    }
    public void Remove(string code)
    {
        if (_rooms.TryRemove(code, out var room))
            ForgetUsers(room);
    }
    public int RemoveIdle(DateTimeOffset cutoff)
    {
        int removed = 0;
        foreach (var entry in _rooms)
            if (entry.Value.IsAbandonedSince(cutoff) && _rooms.TryRemove(entry.Key, out var room))
            {
                ForgetUsers(room);
                removed++;
            }
        return removed;
    }
    private void ForgetUsers(Room room)
    {
        foreach (var member in room.Members())
            _roomByUser.TryRemove(new KeyValuePair<string, Room>(member.UserId, room));
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
