namespace Tripoley.Engine.Tests;

public class MichiganRoundTests
{
	[Fact]
	public void Constructor_InitializesRoundForOpeningLead()
	{
		MichiganRound round = new MichiganRound(CreateEmptyDeal(), 1);

		Assert.Equal(1, round.CurrentLeaderIndex);
		Assert.Null(round.LastPlayedCard);
		Assert.Equal(RoundPhase.AwaitingOpeningLead, round.Phase);
	}

	[Fact]
	public void Constructor_WhenDealIsNull_ThrowsArgumentNullException() =>
		Assert.Throws<ArgumentNullException>(() => new MichiganRound(null!, 0));

	[Theory]
	[InlineData(-1)]
	[InlineData(2)]
	public void Constructor_WhenFirstLeaderIndexIsOutOfRange_Throws(int firstLeaderIndex) =>
		Assert.Throws<ArgumentOutOfRangeException>(() => new MichiganRound(CreateEmptyDeal(), firstLeaderIndex));

	[Theory]
	[InlineData(-1)]
	[InlineData(2)]
	public void TryPlay_WhenPlayerIndexIsOutOfRange_Throws(int playerIndex)
	{
		MichiganRound round = new MichiganRound(CreateEmptyDeal(), 0);

		Assert.Throws<ArgumentOutOfRangeException>(
			() => round.TryPlay(playerIndex, new Card(Suit.Clubs, Rank.Two)));
	}

	[Fact]
	public void TryPlay_OpeningLeaderPlaysLowestCard_RemovesItAndStartsRun()
	{
		Deal deal = CreateEmptyDeal();
		Card openingCard = new Card(Suit.Hearts, Rank.Two);
		Card nextCard = new Card(Suit.Hearts, Rank.Three);
		deal.PlayerHands[0].AddCard(openingCard);
		deal.PlayerHands[0].AddCard(new Card(Suit.Clubs, Rank.Two));
		deal.PlayerHands[1].AddCard(nextCard);
		MichiganRound round = new MichiganRound(deal, 0);

		Assert.True(round.TryPlay(0, openingCard));

		Assert.False(deal.PlayerHands[0].Contains(openingCard));
		Assert.Equal(openingCard, round.LastPlayedCard);
		Assert.Equal(RoundPhase.RunInProgress, round.Phase);
	}

	[Fact]
	public void TryPlay_DuringOpeningLeadRejectsWrongPlayerAndNonLowestCard()
	{
		Deal deal = CreateEmptyDeal();
		Card lowestCard = new Card(Suit.Spades, Rank.Two);
		Card higherCard = new Card(Suit.Spades, Rank.Three);
		Card otherPlayerCard = new Card(Suit.Clubs, Rank.Two);
		deal.PlayerHands[0].AddCard(lowestCard);
		deal.PlayerHands[0].AddCard(higherCard);
		deal.PlayerHands[1].AddCard(otherPlayerCard);
		MichiganRound round = new MichiganRound(deal, 0);

		Assert.False(round.TryPlay(1, otherPlayerCard));
		Assert.False(round.TryPlay(0, higherCard));

		Assert.Equal(RoundPhase.AwaitingOpeningLead, round.Phase);
		Assert.Null(round.LastPlayedCard);
		Assert.True(deal.PlayerHands[0].Contains(lowestCard));
		Assert.True(deal.PlayerHands[0].Contains(higherCard));
	}

	[Fact]
	public void TryPlay_WhenNextCardIsHeldByPlayer_ContinuesRunAndUpdatesLastCard()
	{
		Deal deal = CreateEmptyDeal();
		Card two = new Card(Suit.Hearts, Rank.Two);
		Card three = new Card(Suit.Hearts, Rank.Three);
		Card four = new Card(Suit.Hearts, Rank.Four);
		deal.PlayerHands[0].AddCard(two);
		deal.PlayerHands[0].AddCard(four);
		deal.PlayerHands[1].AddCard(three);
		deal.PlayerHands[1].AddCard(new Card(Suit.Clubs, Rank.Two));
		MichiganRound round = new MichiganRound(deal, 0);

		Assert.True(round.TryPlay(0, two));
		Assert.True(round.TryPlay(1, three));

		Assert.False(deal.PlayerHands[1].Contains(three));
		Assert.Equal(three, round.LastPlayedCard);
		Assert.Equal(RoundPhase.RunInProgress, round.Phase);
	}

	[Fact]
	public void TryPlay_WhenCardSkipsRank_RejectsWithoutChangingState()
	{
		Deal deal = CreateEmptyDeal();
		Card two = new Card(Suit.Hearts, Rank.Two);
		Card four = new Card(Suit.Hearts, Rank.Four);
		deal.PlayerHands[0].AddCard(two);
		deal.PlayerHands[0].AddCard(new Card(Suit.Clubs, Rank.Two));
		deal.PlayerHands[1].AddCard(four);
		MichiganRound round = new MichiganRound(deal, 0);
		round.TryPlay(0, two);

		Assert.False(round.TryPlay(1, four));
		Assert.True(deal.PlayerHands[1].Contains(four));
		Assert.Equal(two, round.LastPlayedCard);
		Assert.Equal(RoundPhase.RunStopped, round.Phase);
	}

	[Fact]
	public void TryPlay_WhenOpeningLeadEmptiesHand_CompletesWithOpeningPlayerAsWinner()
	{
		Deal deal = CreateEmptyDeal();
		Card openingCard = new Card(Suit.Hearts, Rank.Two);
		deal.PlayerHands[0].AddCard(openingCard);
		MichiganRound round = new MichiganRound(deal, 0);

		Assert.True(round.TryPlay(0, openingCard));

		Assert.Equal(RoundPhase.RoundComplete, round.Phase);
		Assert.Equal(0, round.WinnerIndex);
	}

	[Fact]
	public void TryPlay_WhenRunStopsAndLeaderHasAnotherSuit_LeavesThatPlayerAsLeader()
	{
		Deal deal = CreateEmptyDeal();
		Card openingCard = new Card(Suit.Hearts, Rank.Two);
		deal.PlayerHands[0].AddCard(openingCard);
		deal.PlayerHands[0].AddCard(new Card(Suit.Clubs, Rank.Two));
		MichiganRound round = new MichiganRound(deal, 0);

		Assert.True(round.TryPlay(0, openingCard));

		Assert.Equal(RoundPhase.RunStopped, round.Phase);
		Assert.Equal(0, round.CurrentLeaderIndex);
	}

	[Fact]
	public void TryPlay_WhenRunStopsAndLeaderHasNoOtherSuit_SelectsNextEligiblePlayer()
	{
		Deal deal = CreateEmptyDeal();
		Card openingCard = new Card(Suit.Hearts, Rank.Two);
		deal.PlayerHands[0].AddCard(openingCard);
		deal.PlayerHands[0].AddCard(new Card(Suit.Hearts, Rank.King));
		deal.PlayerHands[1].AddCard(new Card(Suit.Clubs, Rank.Two));
		MichiganRound round = new MichiganRound(deal, 0);

		Assert.True(round.TryPlay(0, openingCard));

		Assert.Equal(RoundPhase.RunStopped, round.Phase);
		Assert.Equal(1, round.CurrentLeaderIndex);
	}

	[Fact]
	public void TryPlay_WhenNoPlayerCanLeadAfterStop_CompletesWithoutWinner()
	{
		Deal deal = CreateEmptyDeal();
		Card openingCard = new Card(Suit.Hearts, Rank.Two);
		deal.PlayerHands[0].AddCard(openingCard);
		deal.PlayerHands[0].AddCard(new Card(Suit.Hearts, Rank.King));
		deal.PlayerHands[1].AddCard(new Card(Suit.Hearts, Rank.Five));
		MichiganRound round = new MichiganRound(deal, 0);

		Assert.True(round.TryPlay(0, openingCard));

		Assert.Equal(RoundPhase.RoundComplete, round.Phase);
		Assert.Null(round.WinnerIndex);
	}

	[Fact]
	public void TryPlay_WhenNextCardEmptiesHand_CompletesWithThatPlayerAsWinner()
	{
		Deal deal = CreateEmptyDeal();
		Card two = new Card(Suit.Hearts, Rank.Two);
		Card three = new Card(Suit.Hearts, Rank.Three);
		deal.PlayerHands[0].AddCard(two);
		deal.PlayerHands[0].AddCard(new Card(Suit.Clubs, Rank.Two));
		deal.PlayerHands[1].AddCard(three);
		MichiganRound round = new MichiganRound(deal, 0);

		Assert.True(round.TryPlay(0, two));
		Assert.True(round.TryPlay(1, three));

		Assert.Equal(RoundPhase.RoundComplete, round.Phase);
		Assert.Equal(1, round.WinnerIndex);
		Assert.False(round.TryPlay(0, new Card(Suit.Hearts, Rank.Four)));
	}

	[Fact]
	public void TryPlay_AfterStopRequiresCurrentLeaderAndLowestCardOfDifferentSuit()
	{
		Deal deal = CreateEmptyDeal();
		Card openingCard = new Card(Suit.Hearts, Rank.Two);
		Card sameSuitCard = new Card(Suit.Hearts, Rank.King);
		Card lowestClub = new Card(Suit.Clubs, Rank.Two);
		Card higherClub = new Card(Suit.Clubs, Rank.Three);
		Card otherPlayerCard = new Card(Suit.Spades, Rank.Two);
		deal.PlayerHands[0].AddCard(openingCard);
		deal.PlayerHands[0].AddCard(sameSuitCard);
		deal.PlayerHands[0].AddCard(lowestClub);
		deal.PlayerHands[0].AddCard(higherClub);
		deal.PlayerHands[1].AddCard(otherPlayerCard);
		MichiganRound round = new MichiganRound(deal, 0);
		round.TryPlay(0, openingCard);

		Assert.False(round.TryPlay(1, otherPlayerCard));
		Assert.False(round.TryPlay(0, sameSuitCard));
		Assert.False(round.TryPlay(0, higherClub));
		Assert.Equal(RoundPhase.RunStopped, round.Phase);
		Assert.Equal(openingCard, round.LastPlayedCard);
		Assert.True(deal.PlayerHands[0].Contains(lowestClub));
	}

	[Fact]
	public void TryPlay_AfterStopWithValidDifferentSuitLeadRestartsRun()
	{
		Deal deal = CreateEmptyDeal();
		Card heartsTwo = new Card(Suit.Hearts, Rank.Two);
		Card clubsTwo = new Card(Suit.Clubs, Rank.Two);
		deal.PlayerHands[0].AddCard(heartsTwo);
		deal.PlayerHands[0].AddCard(new Card(Suit.Hearts, Rank.King));
		deal.PlayerHands[0].AddCard(clubsTwo);
		deal.PlayerHands[1].AddCard(new Card(Suit.Clubs, Rank.Three));
		MichiganRound round = new MichiganRound(deal, 0);
		round.TryPlay(0, heartsTwo);

		Assert.True(round.TryPlay(0, clubsTwo));

		Assert.Equal(clubsTwo, round.LastPlayedCard);
		Assert.Equal(RoundPhase.RunInProgress, round.Phase);
	}

	[Fact]
	public void TryPlay_WhenWidowHoldsNextCard_StopsRunBecauseWidowIsNotActive()
	{
		Deal deal = CreateEmptyDeal();
		Card openingCard = new Card(Suit.Hearts, Rank.Two);
		deal.PlayerHands[0].AddCard(openingCard);
		deal.PlayerHands[0].AddCard(new Card(Suit.Clubs, Rank.Two));
		deal.Widow.AddCard(new Card(Suit.Hearts, Rank.Three));
		deal.PlayerHands[1].AddCard(new Card(Suit.Clubs, Rank.Two));
		MichiganRound round = new MichiganRound(deal, 0);

		Assert.True(round.TryPlay(0, openingCard));

		Assert.Equal(RoundPhase.RunStopped, round.Phase);
		Assert.Equal(0, round.CurrentLeaderIndex);
	}

	[Fact]
	public void TryPlay_WhenPlayerIndexIsOutOfRangeAfterRoundComplete_Throws()
	{
		Deal deal = CreateEmptyDeal();
		Card openingCard = new Card(Suit.Hearts, Rank.Two);
		deal.PlayerHands[0].AddCard(openingCard);
		MichiganRound round = new MichiganRound(deal, 0);

		Assert.True(round.TryPlay(0, openingCard));

		Assert.Throws<ArgumentOutOfRangeException>(
			() => round.TryPlay(2, new Card(Suit.Clubs, Rank.Two)));
	}

	private static Deal CreateEmptyDeal()
	{
		Deal deal = Deal.Create(new Deck(), 2);

		foreach (Hand hand in deal.PlayerHands.Append(deal.Widow))
		{
			foreach (Suit suit in Enum.GetValues<Suit>())
			{
				foreach (Rank rank in Enum.GetValues<Rank>())
				{
					hand.TryRemoveCard(new Card(suit, rank));
				}
			}
		}

		return deal;
	}
}