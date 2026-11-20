namespace Tripoley.Engine.Tests;

// Specs for the "Leave Game" engine surface (written before the engine is implemented).
// The only new public members these tests require are Game.Abandon(Guid) and Game.IsAbandoned(Guid);
// every other expectation is observed through existing public state (GamePhase, CurrentMichiganRound,
// CurrentDeal, Pools). The agreed model: a leaver's remaining cards move into the widow, so an
// abandoned seat is simply an empty hand and the existing stop/leader logic handles the rest.
public class GameAbandonTests
{
    [Fact]
    public void Abandon_WhenTwoPlayersAndOneLeaves_SweepsAllPoolsToTheSurvivorAndCompletes()
    {
        Player[] players = [new Player(chipCount: 100, name: "Jeff"), new Player(chipCount: 100, name: "Ann")];
        Game game = CreateDealtGame(players);
        game.StartMichiganRound(firstLeaderIndex: 0);

        // Each player anted 9 (one chip on each of the nine pools), so the layout holds 18 chips.
        game.Abandon(players[1].Id);

        Assert.True(game.IsAbandoned(players[1].Id));
        Assert.Equal(GamePhase.GameComplete, game.GamePhase);
        Assert.Equal(0, TotalPoolChips(game));
        // The leaver forfeits their 91-chip stack into the pools (18 antes + 91 = 109), then the sole
        // remaining player takes every pool. All 200 chips end up with the survivor.
        Assert.Equal(200, players[0].ChipCount);
        Assert.Equal(0, players[1].ChipCount);
    }

    [Fact]
    public void Abandon_WhenGameAlreadyComplete_IsANoOp()
    {
        Player[] players = [new Player(chipCount: 100, name: "Jeff"), new Player(chipCount: 100, name: "Ann")];
        Game game = CreateDealtGame(players);
        game.StartMichiganRound(firstLeaderIndex: 0);
        game.Abandon(players[1].Id);

        // A second abandon on either seat, after the game is over, must change nothing.
        game.Abandon(players[0].Id);
        game.Abandon(players[1].Id);

        Assert.Equal(GamePhase.GameComplete, game.GamePhase);
        Assert.Equal(200, players[0].ChipCount);
        Assert.Equal(0, TotalPoolChips(game));
    }

    [Fact]
    public void Abandon_WhenLeaverHeldTheNextCardMidRun_StopsRunAndPassesLeadToALiveSeat()
    {
        // The H1 hang case: a player can play a card and still hold the next rank, so the run is waiting on
        // their own hand. If that player leaves, their cards go to the widow, the next card is no longer
        // available, and the run must stop with the lead passing to a live seat, never to the leaver.
        Player[] players = CreatePlayers(3);
        Game game = CreateDealtGame(players);
        ReplaceHands(
            game,
            [new Card(Suit.Hearts, Rank.Two), new Card(Suit.Hearts, Rank.Three), new Card(Suit.Clubs, Rank.Two)],
            [new Card(Suit.Clubs, Rank.Five)],
            [new Card(Suit.Spades, Rank.Two)]);
        game.StartMichiganRound(firstLeaderIndex: 0);
        Assert.True(game.TryPlayCard(0, new Card(Suit.Hearts, Rank.Two)));
        Assert.Equal(RoundPhase.RunInProgress, game.CurrentMichiganRound!.Phase);

        game.Abandon(players[0].Id);

        Assert.Equal(RoundPhase.RunStopped, game.CurrentMichiganRound!.Phase);
        Assert.Equal(1, game.CurrentMichiganRound.CurrentLeaderIndex);
        // Even though the leaver held a club and could nominally lead after the stop, the lead must not land
        // on them: their hand is empty (moved to the widow) and they are abandoned.
        Assert.Null(game.CurrentMichiganRound.WinnerIndex);
        Assert.Equal(GamePhase.MichiganRummyInProgress, game.GamePhase);
        Assert.Equal(0, game.CurrentDeal!.Seats[0].Hand.CardsRemaining);
        Assert.Equal(2, game.CurrentDeal.Widow.CardsRemaining);
    }

    [Fact]
    public void Abandon_WhenOpeningLeaderLeavesBeforePlaying_AdvancesToNextSeatThatCanOpen()
    {
        // The M1 case: the opening lead has no previous suit, so advancing the leader must use a plain
        // "has any card" rule, not the suit-restricted after-stop rule. The next seat holds a single suit,
        // which a misapplied suit restriction could wrongly reject.
        Player[] players = CreatePlayers(3);
        Game game = CreateDealtGame(players);
        ReplaceHands(
            game,
            [new Card(Suit.Hearts, Rank.Two)],
            [new Card(Suit.Spades, Rank.Two), new Card(Suit.Spades, Rank.Three)],
            [new Card(Suit.Clubs, Rank.Two)]);
        game.StartMichiganRound(firstLeaderIndex: 0);

        game.Abandon(players[0].Id);

        Assert.Equal(RoundPhase.AwaitingOpeningLead, game.CurrentMichiganRound!.Phase);
        Assert.Equal(1, game.CurrentMichiganRound.CurrentLeaderIndex);
        Assert.Equal(GamePhase.MichiganRummyInProgress, game.GamePhase);
        // The new opening leader can actually lead the lowest card of their only suit.
        Assert.True(game.TryPlayCard(1, new Card(Suit.Spades, Rank.Two)));
    }

    [Fact]
    public void Abandon_WhenLeaveEndsTheRound_SettlesIntoTheKittyThenExcludesTheLeaverFromTheNextDeal()
    {
        // Three players, so losing one is not a walkover. The leaver held the next card and no remaining
        // player can lead a new suit, so the round ends with no winner and settles into the Kitty.
        Player[] players = CreatePlayers(3);
        Game game = CreateDealtGame(players);
        ReplaceHands(
            game,
            [new Card(Suit.Hearts, Rank.Two), new Card(Suit.Hearts, Rank.Three)],
            [new Card(Suit.Hearts, Rank.King)],
            [new Card(Suit.Hearts, Rank.Five)]);
        game.StartMichiganRound(firstLeaderIndex: 0);
        game.TryPlayCard(0, new Card(Suit.Hearts, Rank.Two));

        game.Abandon(players[0].Id);

        Assert.Null(game.CurrentMichiganRound!.WinnerIndex);
        Assert.Equal(GamePhase.WaitingForDeal, game.GamePhase);
        // The leaver forfeits their whole 91-chip stack into the pools; the two remaining players each pay
        // one chip per held card into the Kitty. Pools hold antes (27) + forfeit (91) + per-card (2) = 120.
        Assert.Equal(0, players[0].ChipCount);
        Assert.Equal(90, players[1].ChipCount);
        Assert.Equal(90, players[2].ChipCount);
        Assert.Equal(120, TotalPoolChips(game));

        game.StartNewDeal();

        // The abandoned player is never dealt in again and never antes again.
        Assert.Equal(GamePhase.DealComplete, game.GamePhase);
        Assert.Equal(2, game.CurrentDeal!.Seats.Count);
        Assert.DoesNotContain(players[0], game.CurrentDeal.Seats.Select(seat => seat.Player));
        Assert.Equal(0, players[0].ChipCount);
    }

    [Fact]
    public void Abandon_WhenTheLeaderLeavesAfterAStop_PassesTheLeadToTheNextEligibleSeat()
    {
        Player[] players = CreatePlayers(3);
        Game game = CreateDealtGame(players);
        ReplaceHands(
            game,
            [new Card(Suit.Hearts, Rank.Two), new Card(Suit.Clubs, Rank.Two)],
            [new Card(Suit.Clubs, Rank.Five)],
            [new Card(Suit.Spades, Rank.Two)]);
        game.StartMichiganRound(firstLeaderIndex: 0);
        // Hearts Three is held by no one, so the opening lead stops at once and seat 0 (holding clubs) leads next.
        game.TryPlayCard(0, new Card(Suit.Hearts, Rank.Two));
        Assert.Equal(RoundPhase.RunStopped, game.CurrentMichiganRound!.Phase);
        Assert.Equal(0, game.CurrentMichiganRound.CurrentLeaderIndex);

        game.Abandon(players[0].Id);

        Assert.Equal(RoundPhase.RunStopped, game.CurrentMichiganRound!.Phase);
        Assert.Equal(1, game.CurrentMichiganRound.CurrentLeaderIndex);
        Assert.Equal(GamePhase.MichiganRummyInProgress, game.GamePhase);
    }

    [Fact]
    public void Abandon_WhenAnUninvolvedPlayerLeavesMidRun_TheRunContinuesUnchanged()
    {
        Player[] players = CreatePlayers(3);
        Game game = CreateDealtGame(players);
        ReplaceHands(
            game,
            [new Card(Suit.Hearts, Rank.Two), new Card(Suit.Clubs, Rank.King)],
            [new Card(Suit.Hearts, Rank.Three)],
            [new Card(Suit.Spades, Rank.Two)]);
        game.StartMichiganRound(firstLeaderIndex: 0);
        game.TryPlayCard(0, new Card(Suit.Hearts, Rank.Two));
        Assert.Equal(RoundPhase.RunInProgress, game.CurrentMichiganRound!.Phase);

        // Seat 2 holds none of the run's cards, so their leaving must not stop the run.
        game.Abandon(players[2].Id);

        Assert.Equal(RoundPhase.RunInProgress, game.CurrentMichiganRound!.Phase);
        Assert.Equal(0, game.CurrentMichiganRound.CurrentLeaderIndex);
        Assert.Equal(GamePhase.MichiganRummyInProgress, game.GamePhase);
        // The next card is still live in seat 1, so play can carry on.
        Assert.True(game.TryPlayCard(1, new Card(Suit.Hearts, Rank.Three)));
    }

    [Fact]
    public void Abandon_BetweenRounds_ForfeitsChipsButDoesNotReSettleTheStaleCompletedRound()
    {
        // The guard case for leaving in WaitingForDeal: the previous round is already complete and settled,
        // so CurrentMichiganRound is a stale RoundComplete object. Abandoning now must forfeit the leaver's
        // chips and exclude them from the next deal, but must NOT settle that finished round a second time
        // (which would re-charge the surviving losers for the cards still in their hands).
        Player[] players = CreatePlayers(3);
        Game game = CreateDealtGame(players);
        ReplaceHands(
            game,
            [new Card(Suit.Hearts, Rank.Two)],
            [new Card(Suit.Hearts, Rank.Three), new Card(Suit.Clubs, Rank.Four)],
            [new Card(Suit.Spades, Rank.Five)]);
        game.StartMichiganRound(firstLeaderIndex: 0);

        // Seat 0 leads its only card, empties its hand, and wins. The round settles and the game parks in
        // WaitingForDeal: seat 0 takes the Kitty (3) plus one chip per held card from the two losers (2 + 1),
        // landing on 97; seat 1 pays 2 -> 89; seat 2 pays 1 -> 90. Non-Kitty pools keep their antes (24).
        game.TryPlayCard(0, new Card(Suit.Hearts, Rank.Two));
        Assert.Equal(GamePhase.WaitingForDeal, game.GamePhase);
        Assert.Equal(97, players[0].ChipCount);
        Assert.Equal(89, players[1].ChipCount);
        Assert.Equal(90, players[2].ChipCount);

        game.Abandon(players[2].Id);

        // The stale round is untouched and the game is still waiting to deal, not completed or re-settled.
        Assert.Equal(GamePhase.WaitingForDeal, game.GamePhase);
        Assert.Equal(RoundPhase.RoundComplete, game.CurrentMichiganRound!.Phase);
        Assert.Equal(0, game.CurrentMichiganRound.WinnerIndex);
        Assert.True(game.IsAbandoned(players[2].Id));
        // The winner and the surviving loser are unchanged by the abandon; only the leaver's stack moves,
        // scattered into the pools (24 ante remainder + 90 forfeit = 114). A double-settle would have
        // re-charged seat 1 for its two held cards and paid them to seat 0.
        Assert.Equal(97, players[0].ChipCount);
        Assert.Equal(89, players[1].ChipCount);
        Assert.Equal(0, players[2].ChipCount);
        Assert.Equal(114, TotalPoolChips(game));
    }

    private static Game CreateDealtGame(Player[] players)
    {
        Game game = new Game(players);
        game.StartNewDeal();
        return game;
    }

    private static Player[] CreatePlayers(int count) =>
        [.. Enumerable.Range(1, count).Select(index => new Player(chipCount: 100, name: $"Player {index}"))];

    private static int TotalPoolChips(Game game) =>
        Enum.GetValues<ChipPoolType>().Sum(type => game.Pools.Get(type).ChipCount);

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
