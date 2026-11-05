namespace Tripoley.Api.Contracts;

// A joinable room as shown in the lobby list: identified by code, with its fullness.
public sealed record OpenRoomView(string Code, int PlayerCount, int Capacity);
