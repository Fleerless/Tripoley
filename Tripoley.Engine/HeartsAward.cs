namespace Tripoley.Engine;

public record HeartsAward(ChipPoolType Pool, int SeatIndex, int Chips, IReadOnlyList<Card> Cards);
