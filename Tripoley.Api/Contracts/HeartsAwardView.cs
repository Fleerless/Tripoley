using Tripoley.Engine;

namespace Tripoley.Api.Contracts;

public sealed record HeartsAwardView(ChipPoolType Pool, int SeatIndex, int Chips, IReadOnlyList<CardDto> Cards);
