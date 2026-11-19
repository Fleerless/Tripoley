using Tripoley.Engine;

namespace Tripoley.Api.Rooms;

public sealed class RoomMember(Player player, string userId)
{
    public Player Player { get; } = player;
    public string UserId { get; } = userId;
    public bool IsReady { get; set; } = false;
    public bool IsConnected { get; set; } = true;
}
