using Tripoley.Api.Rooms;

namespace Tripoley.Api.Contracts;

public sealed record RoomView(string Code, RoomState State, IReadOnlyList<RoomMemberView> Members)
{
    public static RoomView FromRoom(Room room)
    {
        var members = room.Members().Select(member => new RoomMemberView(member.Player.Id, member.Player.Name, member.IsReady)).ToList();
        return new RoomView(room.Code, room.State, members);
    }
};
public sealed record RoomMemberView(Guid PlayerId, string Name, bool IsReady);