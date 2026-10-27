namespace Tripoley.Engine.Tests;

public class GameMichiganTests
{
    [Fact]
    public void StartMichiganRound_WhenDealComplete_StartsRoundWithGivenLeader()
    {
        Game game = CreateDealtGame(players: [new Player(chipCount: 100, name: "Jeff"), new Player(chipCount: 100, name: "Ann")]);

        game.StartMichiganRound(firstLeaderIndex: 1);

        Assert.Equal(GamePhase.MichiganRummyInProgress, game.GamePhase);
        Assert.NotNull(game.CurrentMichiganRound);
        Assert.Equal(1, game.CurrentMichiganRound.CurrentLeaderIndex);
    }
    [Fact]
    public void StartMichiganRound_WhenNotDealComplete_ThrowsInvalidOperationException()
    {
        Game game = new Game([new Player(chipCount: 100, name: "Jeff"), new Player(chipCount: 100, name: "Ann")]);

        Assert.Throws<InvalidOperationException>(() => game.StartMichiganRound(0));
    }
    [Fact]
    public void TryPlayCard_WhenNoRoundStarted_ReturnsFalse()
    {
        Game game = CreateDealtGame(players: [new Player(chipCount: 100, name: "Jeff"), new Player(chipCount: 100, name: "Ann")]);

        Assert.False(game.TryPlayCard(0, new Card(Suit.Hearts, Rank.Two)));
    }
    [Fact]
    public void TryPlayCard_WhenPlayerGoesOut_WinnerCollectsKittyPlusAChipPerOpponentCard()
    {
        Player[] players = [new Player(chipCount: 100, name: "Jeff"), new Player(chipCount: 100, name: "Ann")];
        Game game = CreateDealtGame(players);
        ReplaceHands(
            game,
            [new Card(Suit.Hearts, Rank.Two)],
            [new Card(Suit.Clubs, Rank.Two), new Card(Suit.Clubs, Rank.Three), new Card(Suit.Diamonds, Rank.Four)]);
        game.StartMichiganRound(firstLeaderIndex: 0);

        Assert.True(game.TryPlayCard(0, new Card(Suit.Hearts, Rank.Two)));

        // Kitty held 2 antes. Winner collects it, plus 1 chip for each of the 3 cards Ann held.
        Assert.Equal(91 + 2 + 3, players[0].ChipCount);
        Assert.Equal(91 - 3, players[1].ChipCount);
        Assert.Equal(0, game.Pools.Get(ChipPoolType.Kitty).ChipCount);
        Assert.Equal(GamePhase.WaitingForDeal, game.GamePhase);
    }
    [Fact]
    public void TryPlayCard_WhenNoOneCanLeadAfterStop_EveryonePaysPerCardIntoKitty()
    {
        Player[] players = [new Player(chipCount: 100, name: "Jeff"), new Player(chipCount: 100, name: "Ann")];
        Game game = CreateDealtGame(players);
        ReplaceHands(
            game,
            [new Card(Suit.Hearts, Rank.Two), new Card(Suit.Hearts, Rank.King)],
            [new Card(Suit.Hearts, Rank.Five)]);
        game.StartMichiganRound(firstLeaderIndex: 0);

        Assert.True(game.TryPlayCard(0, new Card(Suit.Hearts, Rank.Two)));

        // No winner: Jeff held 1 card, Ann held 1 card, so 2 chips go into the kitty for next deal.
        Assert.Null(game.CurrentMichiganRound!.WinnerIndex);
        Assert.Equal(91 - 1, players[0].ChipCount);
        Assert.Equal(91 - 1, players[1].ChipCount);
        Assert.Equal(2 + 2, game.Pools.Get(ChipPoolType.Kitty).ChipCount);
        Assert.Equal(GamePhase.WaitingForDeal, game.GamePhase);
    }
    [Fact]
    public void TryPlayCard_WhenPlayerCannotCoverPerCardPayment_PaysRemainderToWinner()
    {
        Player[] players = [new Player(chipCount: 100, name: "Jeff"), new Player(chipCount: 20, name: "Ann")];
        Game game = CreateDealtGame(players);
        ReplaceHands(
            game,
            [new Card(Suit.Hearts, Rank.Two)],
            [
                new Card(Suit.Clubs, Rank.Two), new Card(Suit.Clubs, Rank.Three),
                new Card(Suit.Clubs, Rank.Four), new Card(Suit.Clubs, Rank.Five),
                new Card(Suit.Clubs, Rank.Six), new Card(Suit.Clubs, Rank.Seven),
                new Card(Suit.Clubs, Rank.Eight), new Card(Suit.Clubs, Rank.Nine),
                new Card(Suit.Clubs, Rank.Ten), new Card(Suit.Clubs, Rank.Jack),
                new Card(Suit.Clubs, Rank.Queen), new Card(Suit.Clubs, Rank.King),
            ]);
        game.StartMichiganRound(firstLeaderIndex: 0);

        Assert.True(game.TryPlayCard(0, new Card(Suit.Hearts, Rank.Two)));

        // Ann owes 12 chips but only has 11 after the ante, so she pays her remaining 11.
        Assert.Equal(91 + 2 + 11, players[0].ChipCount);
        Assert.Equal(0, players[1].ChipCount);
        Assert.Equal(0, game.Pools.Get(ChipPoolType.Kitty).ChipCount);
    }
    [Fact]
    public void TryPlayCard_WhenPaymentLeavesPlayerBroke_TheyAreIneligibleForTheNextDeal()
    {
        Player[] players = [new Player(chipCount: 100, name: "Jeff"), new Player(chipCount: 20, name: "Ann")];
        Game game = CreateDealtGame(players);
        ReplaceHands(
            game,
            [new Card(Suit.Hearts, Rank.Two)],
            [
                new Card(Suit.Clubs, Rank.Two), new Card(Suit.Clubs, Rank.Three),
                new Card(Suit.Clubs, Rank.Four), new Card(Suit.Clubs, Rank.Five),
                new Card(Suit.Clubs, Rank.Six), new Card(Suit.Clubs, Rank.Seven),
                new Card(Suit.Clubs, Rank.Eight), new Card(Suit.Clubs, Rank.Nine),
                new Card(Suit.Clubs, Rank.Ten), new Card(Suit.Clubs, Rank.Jack),
                new Card(Suit.Clubs, Rank.Queen), new Card(Suit.Clubs, Rank.King),
            ]);
        game.StartMichiganRound(firstLeaderIndex: 0);
        game.TryPlayCard(0, new Card(Suit.Hearts, Rank.Two));

        game.StartNewDeal();

        // With 0 chips Ann cannot cover the ante, and Jeff alone is below the minimum player count.
        Assert.Equal(GamePhase.GameComplete, game.GamePhase);
        Assert.Null(game.CurrentDeal);
    }

    [Fact]
    public void StartMichiganRound_WithoutExplicitLeader_LeadsFromSeatLeftOfTheDealer()
    {
        // On the first deal the dealer is seat 0, so the opening lead falls to the player on their left: seat 1.
        Game game = CreateDealtGame(players:
        [
            new Player(chipCount: 100, name: "Jeff"),
            new Player(chipCount: 100, name: "Ann"),
            new Player(chipCount: 100, name: "Cy"),
        ]);

        game.StartMichiganRound();

        Assert.Equal(GamePhase.MichiganRummyInProgress, game.GamePhase);
        Assert.Equal(1, game.CurrentMichiganRound!.CurrentLeaderIndex);
    }

    [Fact]
    public void StartMichiganRound_WithoutExplicitLeader_SkipsPlayersWhoCouldNotAnte()
    {
        // Broke Betty sits between Jeff and Ann at the table but can't afford the ante, so she isn't dealt in.
        // The dealt seats are [Jeff, Ann]; the dealer is Jeff (seat 0), so the lead passes over Betty to Ann.
        Player[] players =
        [
            new Player(chipCount: 100, name: "Jeff"),
            new Player(chipCount: 5, name: "Broke Betty"),
            new Player(chipCount: 100, name: "Ann"),
        ];
        Game game = CreateDealtGame(players);

        game.StartMichiganRound();

        // The leader index must point into the dealt seats (count 2), not the full table — this is the
        // case that broke when the index was taken from the table's seat order instead of the deal's.
        Assert.Equal(2, game.CurrentDeal!.Seats.Count);
        Assert.Equal(1, game.CurrentMichiganRound!.CurrentLeaderIndex);
        Assert.Equal(players[2], game.CurrentDeal.Seats[1].Player);
    }

    [Fact]
    public void StartNewDeal_RotatesTheDealerClockwise_SoTheOpeningLeadMovesToo()
    {
        Player[] players = [new Player(chipCount: 100, name: "Jeff"), new Player(chipCount: 100, name: "Ann")];
        Game game = CreateDealtGame(players);

        // Deal 1: dealer is seat 0 (Jeff), so Ann (seat 1) leads. Give Ann a single card so she goes out
        // immediately, ending the deal cleanly and returning the game to WaitingForDeal.
        ReplaceHands(game, [new Card(Suit.Clubs, Rank.King)], [new Card(Suit.Hearts, Rank.Two)]);
        game.StartMichiganRound();
        Assert.Equal(1, game.CurrentMichiganRound!.CurrentLeaderIndex);
        Assert.True(game.TryPlayCard(1, new Card(Suit.Hearts, Rank.Two)));
        Assert.Equal(GamePhase.WaitingForDeal, game.GamePhase);

        // Deal 2: the button passes clockwise to Ann, so now Jeff (seat 0) leads.
        game.StartNewDeal();
        game.StartMichiganRound();

        Assert.Equal(0, game.CurrentMichiganRound!.CurrentLeaderIndex);
    }

    private static Game CreateDealtGame(Player[] players)
    {
        Game game = new Game(players);
        game.StartNewDeal();
        return game;
    }

    private static void ReplaceHands(Game game, params Card[][] handsBySeat)
    {
        Deal deal = game.CurrentDeal!;

        foreach (Seat seat in deal.Seats)
        {
            Clear(seat.Hand);
        }
        Clear(deal.Widow);

        for (int seatIndex = 0; seatIndex < handsBySeat.Length; seatIndex++)
        {
            foreach (Card card in handsBySeat[seatIndex])
            {
                deal.Seats[seatIndex].Hand.AddCard(card);
            }
        }
    }

    private static void Clear(Hand hand)
    {
        foreach (Suit suit in Enum.GetValues<Suit>())
        {
            foreach (Rank rank in Enum.GetValues<Rank>())
            {
                hand.TryRemoveCard(new Card(suit, rank));
            }
        }
    }
}
