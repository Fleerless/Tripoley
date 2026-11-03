using Tripoley.Engine;

namespace Tripoley.Api.Rooms;

public sealed class RoomMember(Player player, string connectionId)
{
    public Player Player { get; } = player;
    public string ConnectionId { get; set; } = connectionId;
    public bool IsReady { get; set; } = false;
}
