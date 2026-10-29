using Tripoley.Engine;

namespace Tripoley.Api.Rooms;

public sealed class RoomMember(Player player, string connectionId)
{
    public Player Player { get; } = player;                  // engine identity + chips; created when the player joins
    public string ConnectionId { get; set; } = connectionId; // SignalR socket; changes on reconnect
    public bool IsReady { get; set; }
}
