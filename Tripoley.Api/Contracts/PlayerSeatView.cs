namespace Tripoley.Api.Contracts;

public sealed record PlayerSeatView(int SeatIndex, string Name, int ChipCount, int CardsRemaining);