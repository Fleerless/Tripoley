using Tripoley.Engine;

namespace Tripoley.Api.Contracts;

public sealed record GameStateView(
    GamePhase Phase,
    int? CurrentLeaderIndex,
    CardDto? LastPlayedCard,
    int? WinnerIndex,
    IReadOnlyList<PoolView> Pools,
    IReadOnlyList<PlayerSeatView> Seats
);