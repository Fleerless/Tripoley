namespace Tripoley.Api.Hubs;

using Microsoft.AspNetCore.SignalR;
using Tripoley.Api.Rooms;

public sealed class GameHub(IRoomRegistry roomRegistry) : Hub
{
}