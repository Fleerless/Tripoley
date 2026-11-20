namespace Tripoley.Api.Rooms;

public interface IRoomRegistry
{
    Room Create();
    Room? Find(string code);
    Room? FindByUserId(string userId);
    Room? Leave(string userId);
    IReadOnlyList<Room> OpenRooms();
    void Remove(string code);
    int RemoveIdle(DateTimeOffset cutoff);
}
