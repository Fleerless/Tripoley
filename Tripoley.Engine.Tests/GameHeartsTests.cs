namespace Tripoley.Engine.Tests;

// Specs for the Game-level Hearts transition, written before the engine is implemented.
// New engine members: Game.StartHeartsPhase(), Game.CurrentHeartsAwards, Game.DealNumber.
// The flow per deal is DealComplete -> (StartHeartsPhase) HeartsInProgress -> (StartMichiganRound)
// MichiganRummyInProgress, so the StartMichiganRound guard now requires the Hearts phase to have run.
public class GameHeartsTests
{
    [Fact]
    public void StartHeartsPhase_WhenDealComplete_SettlesThePoolsAndEntersHeartsInProgress()
    {
        Player[] players = [new Player(chipCount: 100, name: "Jeff"), new Player(chipCount: 100, name: "Ann")];
        Game game = CreateDealtGame(players);
        ReplaceHands(game, [new Card(Suit.Hearts, Rank.Ace)], [new Card(Suit.Clubs, Rank.Two)]);

        game.StartHeartsPhase();

        Assert.Equal(GamePhase.HeartsInProgress, game.GamePhase);
        Assert.NotNull(game.CurrentHeartsAwards);
        // Jeff held the Ace of hearts, so the Ace pool (2 antes) moved to him and is now empty.
        Assert.Contains(game.CurrentHeartsAwards!, a => a.Pool == ChipPoolType.AceOfHearts && a.SeatIndex == 0);
        Assert.Equal(0, game.Pools.Get(ChipPoolType.AceOfHearts).ChipCount);
    }

    [Fact]
    public void StartHeartsPhase_WhenNotDealComplete_ThrowsInvalidOperationException()
    {
        Game game = new Game([new Player(chipCount: 100, name: "Jeff"), new Player(chipCount: 100, name: "Ann")]);

        Assert.Throws<InvalidOperationException>(() => game.StartHeartsPhase());
    }

    [Fact]
    public void StartHeartsPhase_WithNoPayCardHolders_SettlesWithAnEmptyAwardListAndTouchesNoPools()
    {
        Player[] players = [new Player(chipCount: 100, name: "Jeff"), new Player(chipCount: 100, name: "Ann")];
        Game game = CreateDealtGame(players);
        ReplaceHands(game, [new Card(Suit.Clubs, Rank.Two)], [new Card(Suit.Spades, Rank.Three)]);

        game.StartHeartsPhase();

        Assert.NotNull(game.CurrentHeartsAwards);
        Assert.Empty(game.CurrentHeartsAwards!);
        Assert.All(Enum.GetValues<ChipPoolType>(), type => Assert.Equal(2, game.Pools.Get(type).ChipCount));
    }

    [Fact]
    public void StartMichiganRound_BeforeTheHeartsPhaseHasRun_ThrowsInvalidOperationException()
    {
        Player[] players = [new Player(chipCount: 100, name: "Jeff"), new Player(chipCount: 100, name: "Ann")];
        Game game = CreateDealtGame(players); // DealComplete, Hearts not yet run

        Assert.Throws<InvalidOperationException>(() => game.StartMichiganRound(0));
    }

    [Fact]
    public void StartMichiganRound_AfterTheHeartsPhase_StartsTheRound()
    {
        Player[] players = [new Player(chipCount: 100, name: "Jeff"), new Player(chipCount: 100, name: "Ann")];
        Game game = CreateDealtGame(players);
        game.StartHeartsPhase();

        game.StartMichiganRound(firstLeaderIndex: 1);

        Assert.Equal(GamePhase.MichiganRummyInProgress, game.GamePhase);
        Assert.Equal(1, game.CurrentMichiganRound!.CurrentLeaderIndex);
    }

    [Fact]
    public void DealNumber_IsZeroBeforeAnyDealAndIncrementsOnEachRealDeal()
    {
        Player[] players = [new Player(chipCount: 100, name: "Jeff"), new Player(chipCount: 100, name: "Ann")];
        Game game = new Game(players);
        Assert.Equal(0, game.DealNumber);

        game.StartNewDeal();
        Assert.Equal(1, game.DealNumber);

        // Finish the deal cleanly so the game returns to WaitingForDeal, then deal again.
        ReplaceHands(game, [new Card(Suit.Clubs, Rank.King)], [new Card(Suit.Hearts, Rank.Two)]);
        game.StartHeartsPhase();
        game.StartMichiganRound(firstLeaderIndex: 1);
        Assert.True(game.TryPlayCard(1, new Card(Suit.Hearts, Rank.Two)));
        Assert.Equal(GamePhase.WaitingForDeal, game.GamePhase);

        game.StartNewDeal();
        Assert.Equal(2, game.DealNumber);
    }

    [Fact]
    public void DealNumber_DoesNotIncrementWhenTheDealEndsTheGame()
    {
        // Only one player can cover the ante, so StartNewDeal completes the game without dealing.
        Player[] players = [new Player(chipCount: 100, name: "Jeff"), new Player(chipCount: 5, name: "Broke Betty")];
        Game game = new Game(players);

        game.StartNewDeal();

        Assert.Equal(GamePhase.GameComplete, game.GamePhase);
        Assert.Equal(0, game.DealNumber);
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
        foreach (Seat seat in deal.Seats) Clear(seat.Hand);
        Clear(deal.Widow);
        for (int seatIndex = 0; seatIndex < handsBySeat.Length; seatIndex++)
            foreach (Card card in handsBySeat[seatIndex])
                deal.Seats[seatIndex].Hand.AddCard(card);
    }

    private static void Clear(Hand hand)
    {
        foreach (Suit suit in Enum.GetValues<Suit>())
            foreach (Rank rank in Enum.GetValues<Rank>())
                hand.TryRemoveCard(new Card(suit, rank));
    }
}
