using Tripoley.Engine;

namespace Tripoley.Api.Contracts;

public sealed record GameStateView(
    GamePhase Phase,
    RoundPhase? RoundPhase,
    int? CurrentLeaderIndex,
    int? CurrentTurnSeatIndex,
    CardDto? LastPlayedCard,
    CardDto? NextCard,
    int? WinnerIndex,
    IReadOnlyList<PoolView> Pools,
    IReadOnlyList<PlayerSeatView> Seats
);