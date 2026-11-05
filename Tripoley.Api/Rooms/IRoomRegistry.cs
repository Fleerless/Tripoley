namespace Tripoley.Api.Rooms;

public interface IRoomRegistry
{
    Room Create();
    Room? Find(string code);
    Room? FindByConnectionId(string connectionId);
    IReadOnlyList<Room> OpenRooms();
    void Remove(string code);
}