namespace Tripoley.Api.Contracts;

public sealed record SelfView(
    IReadOnlyList<CardDto> Hand
);