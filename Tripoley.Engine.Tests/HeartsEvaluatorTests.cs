namespace Tripoley.Engine.Tests;

// Specs for the Hearts (pay-card) settlement, written before the engine is implemented
// (mirrors the GameAbandonTests "spec-first" approach). The engine members these pin are:
//   - static HeartsEvaluator.Settle(Deal, ChipPools) -> IReadOnlyList<HeartsAward>
//   - record HeartsAward(ChipPoolType Pool, int SeatIndex, int Chips, IReadOnlyList<Card> Cards)
// Settle applies the chip movements to the pools/players and returns one award per pool paid
// out. The 8-9-10 pool can produce several awards (one per qualifying seat), so awards is a
// flat list. The revealed Cards on each award are the pay cards that justify it, so the UI can
// show why chips moved without ever receiving a full hand.
public class HeartsEvaluatorTests
{
    [Fact]
    public void Settle_SingleHeartHolder_TakesTheWholePoolAndRecordsTheCard()
    {
        (Deal deal, Player[] players) = DealWithEmptyHands(seatCount: 2);
        deal.Seats[0].Hand.AddCard(new Card(Suit.Hearts, Rank.Ace));
        ChipPools pools = PoolsWithChipsOnEverySpace(chipsEach: 2);

        IReadOnlyList<HeartsAward> awards = HeartsEvaluator.Settle(deal, pools);

        Assert.Equal(2, players[0].ChipCount);
        Assert.Equal(0, pools.Get(ChipPoolType.AceOfHearts).ChipCount);
        HeartsAward ace = Assert.Single(awards, a => a.Pool == ChipPoolType.AceOfHearts);
        Assert.Equal(0, ace.SeatIndex);
        Assert.Equal(2, ace.Chips);
        Assert.Equal(new[] { new Card(Suit.Hearts, Rank.Ace) }, ace.Cards);
    }

    [Fact]
    public void Settle_WhenOneSeatHoldsBothKingAndQueen_TakesKingQueenAndQueenAndKingPools()
    {
        (Deal deal, Player[] players) = DealWithEmptyHands(seatCount: 2);
        deal.Seats[0].Hand.AddCard(new Card(Suit.Hearts, Rank.King));
        deal.Seats[0].Hand.AddCard(new Card(Suit.Hearts, Rank.Queen));
        ChipPools pools = PoolsWithChipsOnEverySpace(chipsEach: 2);

        IReadOnlyList<HeartsAward> awards = HeartsEvaluator.Settle(deal, pools);

        // King (2) + Queen (2) + King-Queen (2) all land on seat 0.
        Assert.Equal(6, players[0].ChipCount);
        Assert.Equal(0, pools.Get(ChipPoolType.KingOfHearts).ChipCount);
        Assert.Equal(0, pools.Get(ChipPoolType.QueenOfHearts).ChipCount);
        Assert.Equal(0, pools.Get(ChipPoolType.KingQueenOfHearts).ChipCount);
        Assert.All(
            new[] { ChipPoolType.KingOfHearts, ChipPoolType.QueenOfHearts, ChipPoolType.KingQueenOfHearts },
            pool => Assert.Contains(awards, a => a.Pool == pool && a.SeatIndex == 0));
        HeartsAward combo = Assert.Single(awards, a => a.Pool == ChipPoolType.KingQueenOfHearts);
        Assert.Equal(
            new[] { new Card(Suit.Hearts, Rank.King), new Card(Suit.Hearts, Rank.Queen) },
            combo.Cards);
    }

    [Fact]
    public void Settle_WhenKingAndQueenAreSplitAcrossSeats_LeavesTheKingQueenPoolUntouched()
    {
        (Deal deal, Player[] players) = DealWithEmptyHands(seatCount: 2);
        deal.Seats[0].Hand.AddCard(new Card(Suit.Hearts, Rank.King));
        deal.Seats[1].Hand.AddCard(new Card(Suit.Hearts, Rank.Queen));
        ChipPools pools = PoolsWithChipsOnEverySpace(chipsEach: 2);

        IReadOnlyList<HeartsAward> awards = HeartsEvaluator.Settle(deal, pools);

        Assert.Equal(2, players[0].ChipCount);
        Assert.Equal(2, players[1].ChipCount);
        // No single seat holds both, so the combo pool carries over.
        Assert.Equal(2, pools.Get(ChipPoolType.KingQueenOfHearts).ChipCount);
        Assert.DoesNotContain(awards, a => a.Pool == ChipPoolType.KingQueenOfHearts);
    }

    [Fact]
    public void Settle_EightNineTenSingleHolder_TakesTheWholePoolWithTheSuitedRun()
    {
        (Deal deal, Player[] players) = DealWithEmptyHands(seatCount: 2);
        AddRun(deal.Seats[0].Hand, Suit.Clubs);
        ChipPools pools = PoolsWithChipsOnEverySpace(chipsEach: 2);

        IReadOnlyList<HeartsAward> awards = HeartsEvaluator.Settle(deal, pools);

        Assert.Equal(2, players[0].ChipCount);
        Assert.Equal(0, pools.Get(ChipPoolType.EightNineTen).ChipCount);
        HeartsAward run = Assert.Single(awards, a => a.Pool == ChipPoolType.EightNineTen);
        Assert.Equal(0, run.SeatIndex);
        Assert.Equal(2, run.Chips);
        Assert.Equal(
            new[]
            {
                new Card(Suit.Clubs, Rank.Eight),
                new Card(Suit.Clubs, Rank.Nine),
                new Card(Suit.Clubs, Rank.Ten),
            },
            run.Cards);
    }

    [Fact]
    public void Settle_EightNineTenTwoHolders_SplitsEvenlyAndLeavesTheRemainderInThePool()
    {
        (Deal deal, Player[] players) = DealWithEmptyHands(seatCount: 2);
        AddRun(deal.Seats[0].Hand, Suit.Clubs);
        AddRun(deal.Seats[1].Hand, Suit.Spades);
        // 3 chips, 2 qualifiers: each takes 1, the odd chip stays on the layout.
        ChipPools pools = PoolsWithChipsOnEverySpace(chipsEach: 3);

        IReadOnlyList<HeartsAward> awards = HeartsEvaluator.Settle(deal, pools);

        Assert.Equal(1, players[0].ChipCount);
        Assert.Equal(1, players[1].ChipCount);
        Assert.Equal(1, pools.Get(ChipPoolType.EightNineTen).ChipCount);
        Assert.Equal(
            2,
            awards.Count(a => a.Pool == ChipPoolType.EightNineTen && a.Chips == 1));
    }

    [Fact]
    public void Settle_WhenNoSeatHoldsAnEightNineTenRun_LeavesThePoolUntouched()
    {
        (Deal deal, Player[] players) = DealWithEmptyHands(seatCount: 2);
        deal.Seats[0].Hand.AddCard(new Card(Suit.Clubs, Rank.Eight));
        deal.Seats[0].Hand.AddCard(new Card(Suit.Clubs, Rank.Nine));
        deal.Seats[1].Hand.AddCard(new Card(Suit.Clubs, Rank.Ten));
        ChipPools pools = PoolsWithChipsOnEverySpace(chipsEach: 2);

        IReadOnlyList<HeartsAward> awards = HeartsEvaluator.Settle(deal, pools);

        Assert.Equal(0, players[0].ChipCount);
        Assert.Equal(0, players[1].ChipCount);
        Assert.Equal(2, pools.Get(ChipPoolType.EightNineTen).ChipCount);
        Assert.DoesNotContain(awards, a => a.Pool == ChipPoolType.EightNineTen);
    }

    [Fact]
    public void Settle_WhenThePayCardIsInTheWidow_LeavesThatPoolUntouched()
    {
        (Deal deal, Player[] players) = DealWithEmptyHands(seatCount: 2);
        // The King of hearts was dealt to the widow, so no seat holds it.
        deal.Widow.AddCard(new Card(Suit.Hearts, Rank.King));
        ChipPools pools = PoolsWithChipsOnEverySpace(chipsEach: 2);

        IReadOnlyList<HeartsAward> awards = HeartsEvaluator.Settle(deal, pools);

        Assert.All(players, p => Assert.Equal(0, p.ChipCount));
        Assert.Equal(2, pools.Get(ChipPoolType.KingOfHearts).ChipCount);
        Assert.DoesNotContain(awards, a => a.Pool == ChipPoolType.KingOfHearts);
    }

    [Fact]
    public void Settle_WhenAPoolIsEmpty_PaysNothingAndRecordsNoAward()
    {
        (Deal deal, Player[] players) = DealWithEmptyHands(seatCount: 2);
        deal.Seats[0].Hand.AddCard(new Card(Suit.Hearts, Rank.Ace));
        ChipPools pools = new ChipPools(); // never anted: every pool holds 0

        IReadOnlyList<HeartsAward> awards = HeartsEvaluator.Settle(deal, pools);

        Assert.Equal(0, players[0].ChipCount);
        Assert.Empty(awards);
    }

    private static (Deal deal, Player[] players) DealWithEmptyHands(int seatCount)
    {
        // Deal players start with 0 chips so a post-settlement count IS exactly what they won.
        Player[] players = Enumerable.Range(0, seatCount)
            .Select(i => new Player(chipCount: 0, name: $"P{i}"))
            .ToArray();
        Deck deck = new Deck(new Random(0));
        Deal deal = Deal.Create(deck, players);

        foreach (Seat seat in deal.Seats) Clear(seat.Hand);
        Clear(deal.Widow);
        return (deal, players);
    }

    // AddAnte puts one chip on every space, so N throwaway antes load every pool to N.
    private static ChipPools PoolsWithChipsOnEverySpace(int chipsEach)
    {
        ChipPools pools = new ChipPools();
        for (int i = 0; i < chipsEach; i++)
            pools.AddAnte(new Player(chipCount: pools.AnteCost, name: "ante"));
        return pools;
    }

    private static void AddRun(Hand hand, Suit suit)
    {
        hand.AddCard(new Card(suit, Rank.Eight));
        hand.AddCard(new Card(suit, Rank.Nine));
        hand.AddCard(new Card(suit, Rank.Ten));
    }

    private static void Clear(Hand hand)
    {
        foreach (Suit suit in Enum.GetValues<Suit>())
            foreach (Rank rank in Enum.GetValues<Rank>())
                hand.TryRemoveCard(new Card(suit, rank));
    }
}
