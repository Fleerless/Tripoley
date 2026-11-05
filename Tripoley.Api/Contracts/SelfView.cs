namespace Tripoley.Api.Contracts;

public sealed record SelfView(
    int? SeatIndex,
    IReadOnlyList<CardDto> Hand,
    IReadOnlyList<CardDto> LegalPlays
);