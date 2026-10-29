using Tripoley.Engine;

namespace Tripoley.Api.Contracts;

public sealed record PoolView(
    ChipPoolType Type,
    int ChipCount
);