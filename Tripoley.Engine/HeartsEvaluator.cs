namespace Tripoley.Engine;

public static class HeartsEvaluator
{
    public static IReadOnlyList<HeartsAward> Settle(Deal deal, ChipPools pools)
    {
        ArgumentNullException.ThrowIfNull(deal);
        ArgumentNullException.ThrowIfNull(pools);

        var awards = new List<HeartsAward>();
        AwardUniqueHeart(deal, pools, ChipPoolType.AceOfHearts, Rank.Ace, awards);
        AwardUniqueHeart(deal, pools, ChipPoolType.KingOfHearts, Rank.King, awards);
        AwardUniqueHeart(deal, pools, ChipPoolType.QueenOfHearts, Rank.Queen, awards);
        AwardUniqueHeart(deal, pools, ChipPoolType.JackOfHearts, Rank.Jack, awards);
        AwardUniqueHeart(deal, pools, ChipPoolType.TenOfHearts, Rank.Ten, awards);
        AwardKingQueen(deal, pools, awards);
        AwardEightNineTen(deal, pools, awards);
        return awards;
    }

    private static void AwardUniqueHeart(Deal deal, ChipPools pools, ChipPoolType type, Rank rank, List<HeartsAward> awards)
    {
        Card card = new Card(Suit.Hearts, rank);
        int seatIndex = IndexOfHolder(deal, card);
        if (seatIndex < 0) return;

        int chips = pools.Get(type).ChipCount;
        if (chips == 0) return;

        pools.ClaimChips(type, deal.Seats[seatIndex].Player);
        awards.Add(new HeartsAward(type, seatIndex, chips, [card]));
    }

    private static void AwardKingQueen(Deal deal, ChipPools pools, List<HeartsAward> awards)
    {
        Card king = new Card(Suit.Hearts, Rank.King);
        Card queen = new Card(Suit.Hearts, Rank.Queen);

        int seatIndex = IndexOfHolder(deal, king);
        if (seatIndex < 0 || seatIndex != IndexOfHolder(deal, queen)) return;

        int chips = pools.Get(ChipPoolType.KingQueenOfHearts).ChipCount;
        if (chips == 0) return;

        pools.ClaimChips(ChipPoolType.KingQueenOfHearts, deal.Seats[seatIndex].Player);
        awards.Add(new HeartsAward(ChipPoolType.KingQueenOfHearts, seatIndex, chips, [king, queen]));
    }

    private static void AwardEightNineTen(Deal deal, ChipPools pools, List<HeartsAward> awards)
    {
        var holders = new List<(int Index, Suit Suit)>();
        for (int index = 0; index < deal.Seats.Count; index++)
        {
            if (deal.Seats[index].Hand.EightNineTenSuit() is { } suit)
            {
                holders.Add((index, suit));
            }
        }
        if (holders.Count == 0) return;

        var recipients = holders.Select(holder => deal.Seats[holder.Index].Player).ToList();
        int each = pools.SplitChipsEvenly(ChipPoolType.EightNineTen, recipients);
        if (each == 0) return;

        foreach ((int index, Suit suit) in holders)
        {
            Card[] run =
            [
                new Card(suit, Rank.Eight),
                new Card(suit, Rank.Nine),
                new Card(suit, Rank.Ten),
            ];
            awards.Add(new HeartsAward(ChipPoolType.EightNineTen, index, each, run));
        }
    }

    private static int IndexOfHolder(Deal deal, Card card)
    {
        for (int index = 0; index < deal.Seats.Count; index++)
        {
            if (deal.Seats[index].Hand.Contains(card)) return index;
        }
        return -1;
    }
}
