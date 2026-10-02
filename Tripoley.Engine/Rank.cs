namespace Tripoley.Engine;

public enum Rank
{
    Two,
    Three,
    Four,
    Five,
    Six,
    Seven,
    Eight,
    Nine,
    Ten,
    Jack,
    Queen,
    King,
    Ace
}
public static class RankExtensions
{
public static int GetValue(this Rank rank) =>
    rank switch
    {
        Rank.Two => 2,
        Rank.Three => 3,
        Rank.Four => 4,
        Rank.Five => 5,
        Rank.Six => 6,
        Rank.Seven => 7,
        Rank.Eight => 8,
        Rank.Nine => 9,
        Rank.Ten => 10,
        Rank.Jack => 11,
        Rank.Queen => 12,
        Rank.King => 13,
        Rank.Ace => 14,
        _ => throw new ArgumentOutOfRangeException(nameof(rank))
    };
}