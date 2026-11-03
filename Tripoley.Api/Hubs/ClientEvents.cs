namespace Tripoley.Api.Hubs;

/// <summary>
/// Names of the messages the server pushes to clients. These are the wire contract the
/// client subscribes to by the exact same string, so they live in one place to avoid drift.
/// </summary>
public static class ClientEvents
{
    public const string LobbyUpdated = "LobbyUpdated";
    public const string GameStateUpdated = "GameStateUpdated";
    public const string HandUpdated = "HandUpdated";
}
