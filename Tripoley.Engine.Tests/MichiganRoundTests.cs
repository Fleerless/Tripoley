namespace Tripoley.Engine.Tests;

public class MichiganRoundTests
{
	[Fact]
	public void Constructor_WhenInputsAreValid_StoresDealAndFirstLeader()
	{
		Deal deal = CreateEmptyDeal();

		MichiganRound round = new MichiganRound(deal, 1);

		Assert.Equal(1, round.CurrentLeaderIndex);
		Assert.Null(round.LastPlayedCard);
	}

	[Fact]
	public void Constructor_WhenDealIsNull_ThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => new MichiganRound(null!, 0));
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(2)]
	public void Constructor_WhenFirstLeaderIndexIsOutOfRange_Throws(int firstLeaderIndex)
	{
		Deal deal = CreateEmptyDeal();

		Assert.Throws<ArgumentOutOfRangeException>(() => new MichiganRound(deal, firstLeaderIndex));
	}

	[Fact]
	public void TryLeadOpeningCard_WhenLeaderPlaysLowestCardInSuit_RemovesAndRecordsCard()
	{
		Deal deal = CreateEmptyDeal();
		Card openingCard = new Card(Suit.Hearts, Rank.Two);
		deal.PlayerHands[0].AddCard(openingCard);
		deal.PlayerHands[0].AddCard(new Card(Suit.Hearts, Rank.King));
		MichiganRound round = new MichiganRound(deal, 0);

		bool result = round.TryLeadOpeningCard(0, openingCard);

		Assert.True(result);
		Assert.False(deal.PlayerHands[0].Contains(openingCard));
		Assert.Equal(openingCard, round.LastPlayedCard);
	}

	[Fact]
	public void TryLeadOpeningCard_WhenAnotherPlayerAttemptsToLead_ReturnsFalseWithoutChangingState()
	{
		Deal deal = CreateEmptyDeal();
		Card attemptedCard = new Card(Suit.Clubs, Rank.Two);
		deal.PlayerHands[1].AddCard(attemptedCard);
		MichiganRound round = new MichiganRound(deal, 0);

		bool result = round.TryLeadOpeningCard(1, attemptedCard);

		Assert.False(result);
		Assert.True(deal.PlayerHands[1].Contains(attemptedCard));
		Assert.Null(round.LastPlayedCard);
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(2)]
	public void TryLeadOpeningCard_WhenPlayerIndexIsOutOfRange_Throws(int playerIndex)
	{
		Deal deal = CreateEmptyDeal();
		MichiganRound round = new MichiganRound(deal, 0);

		Assert.Throws<ArgumentOutOfRangeException>(
			() => round.TryLeadOpeningCard(playerIndex, new Card(Suit.Clubs, Rank.Two)));
	}

	[Fact]
	public void TryLeadOpeningCard_WhenCardIsNotLowestInSuit_ReturnsFalseWithoutChangingState()
	{
		Deal deal = CreateEmptyDeal();
		Card lowestCard = new Card(Suit.Spades, Rank.Two);
		Card attemptedCard = new Card(Suit.Spades, Rank.Three);
		deal.PlayerHands[0].AddCard(lowestCard);
		deal.PlayerHands[0].AddCard(attemptedCard);
		MichiganRound round = new MichiganRound(deal, 0);

		bool result = round.TryLeadOpeningCard(0, attemptedCard);

		Assert.False(result);
		Assert.True(deal.PlayerHands[0].Contains(lowestCard));
		Assert.True(deal.PlayerHands[0].Contains(attemptedCard));
		Assert.Null(round.LastPlayedCard);
	}

	[Fact]
	public void TryLeadOpeningCard_WhenOpeningCardWasAlreadyPlayed_ReturnsFalse()
	{
		Deal deal = CreateEmptyDeal();
		Card openingCard = new Card(Suit.Diamonds, Rank.Two);
		Card secondCard = new Card(Suit.Diamonds, Rank.Three);
		deal.PlayerHands[0].AddCard(openingCard);
		deal.PlayerHands[0].AddCard(secondCard);
		MichiganRound round = new MichiganRound(deal, 0);
		Assert.True(round.TryLeadOpeningCard(0, openingCard));

		bool result = round.TryLeadOpeningCard(0, secondCard);

		Assert.False(result);
		Assert.True(deal.PlayerHands[0].Contains(secondCard));
		Assert.Equal(openingCard, round.LastPlayedCard);
	}

	[Fact]
	public void TryPlayNextCard_WhenNoOpeningCardHasBeenPlayed_ReturnsFalse()
	{
		Deal deal = CreateEmptyDeal();
		MichiganRound round = new MichiganRound(deal, 0);

		bool result = round.TryPlayNextCard(1, new Card(Suit.Hearts, Rank.Three));

		Assert.False(result);
		Assert.Null(round.LastPlayedCard);
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(2)]
	public void TryPlayNextCard_WhenPlayerIndexIsOutOfRange_ThrowsEvenBeforeOpeningLead(int playerIndex)
	{
		Deal deal = CreateEmptyDeal();
		MichiganRound round = new MichiganRound(deal, 0);

		Assert.Throws<ArgumentOutOfRangeException>(
			() => round.TryPlayNextCard(playerIndex, new Card(Suit.Hearts, Rank.Three)));
	}

	[Fact]
	public void TryPlayNextCard_WhenNextCardIsHeldByPlayer_RemovesAndRecordsCard()
	{
		Deal deal = CreateEmptyDeal();
		Card openingCard = new Card(Suit.Hearts, Rank.Two);
		Card nextCard = new Card(Suit.Hearts, Rank.Three);
		deal.PlayerHands[0].AddCard(openingCard);
		deal.PlayerHands[1].AddCard(nextCard);
		MichiganRound round = new MichiganRound(deal, 0);
		Assert.True(round.TryLeadOpeningCard(0, openingCard));

		bool result = round.TryPlayNextCard(1, nextCard);

		Assert.True(result);
		Assert.False(deal.PlayerHands[1].Contains(nextCard));
		Assert.Equal(nextCard, round.LastPlayedCard);
	}

	[Fact]
	public void TryPlayNextCard_WhenPlayersPlayTwoConsecutiveCards_ValidatesAgainstLastPlayedAndUpdatesState()
	{
		Deal deal = CreateEmptyDeal();
		Card openingCard = new Card(Suit.Hearts, Rank.Two);
		Card secondCard = new Card(Suit.Hearts, Rank.Three);
		Card thirdCard = new Card(Suit.Hearts, Rank.Four);
        Card fourthCard = new Card(Suit.Hearts, Rank.Five);
		deal.PlayerHands[0].AddCard(openingCard);
		deal.PlayerHands[0].AddCard(thirdCard);
		deal.PlayerHands[1].AddCard(secondCard);
        deal.PlayerHands[1].AddCard(fourthCard);
		MichiganRound round = new MichiganRound(deal, 0);

		Assert.True(round.TryLeadOpeningCard(0, openingCard));

		bool secondPlayResult = round.TryPlayNextCard(1, secondCard);
		bool thirdPlayResult = round.TryPlayNextCard(0, thirdCard);

		Assert.True(secondPlayResult);
		Assert.True(thirdPlayResult);
		Assert.False(deal.PlayerHands[1].Contains(secondCard));
		Assert.False(deal.PlayerHands[0].Contains(thirdCard));
		Assert.Equal(thirdCard, round.LastPlayedCard);
	}

	[Fact]
	public void TryPlayNextCard_WhenCardSkipsRank_ReturnsFalseWithoutChangingState()
	{
		Deal deal = CreateEmptyDeal();
		Card openingCard = new Card(Suit.Hearts, Rank.Two);
		Card skippedCard = new Card(Suit.Hearts, Rank.Four);
		deal.PlayerHands[0].AddCard(openingCard);
		deal.PlayerHands[1].AddCard(skippedCard);
		MichiganRound round = new MichiganRound(deal, 0);
		Assert.True(round.TryLeadOpeningCard(0, openingCard));

		bool result = round.TryPlayNextCard(1, skippedCard);

		Assert.False(result);
		Assert.True(deal.PlayerHands[1].Contains(skippedCard));
		Assert.Equal(openingCard, round.LastPlayedCard);
	}

	[Fact]
	public void TryPlayNextCard_WhenNextCardIsInAnotherPlayersHand_ReturnsFalseWithoutChangingState()
	{
		Deal deal = CreateEmptyDeal();
		Card openingCard = new Card(Suit.Hearts, Rank.Two);
		Card nextCard = new Card(Suit.Hearts, Rank.Three);
		deal.PlayerHands[0].AddCard(openingCard);
		deal.PlayerHands[0].AddCard(nextCard);
		MichiganRound round = new MichiganRound(deal, 0);
		Assert.True(round.TryLeadOpeningCard(0, openingCard));

		bool result = round.TryPlayNextCard(1, nextCard);

		Assert.False(result);
		Assert.True(deal.PlayerHands[0].Contains(nextCard));
		Assert.Equal(openingCard, round.LastPlayedCard);
	}

	[Fact]
	public void TryLeadOpeningCard_WhenOpeningCardEmptiesHand_CompletesRoundWithOpeningPlayerAsWinner()
	{
		Deal deal = CreateEmptyDeal();
		Card openingCard = new Card(Suit.Hearts, Rank.Two);
		deal.PlayerHands[0].AddCard(openingCard);
		deal.PlayerHands[1].AddCard(new Card(Suit.Hearts, Rank.Three));
		MichiganRound round = new MichiganRound(deal, 0);

		bool result = round.TryLeadOpeningCard(0, openingCard);

		Assert.True(result);
		Assert.True(round.IsRoundComplete);
		Assert.Equal(0, round.WinnerIndex);
	}

	[Fact]
	public void TryLeadOpeningCard_WhenRunStopsAndLeaderCanLeadAnotherSuit_KeepsCurrentLeader()
	{
		Deal deal = CreateEmptyDeal();
		Card openingCard = new Card(Suit.Hearts, Rank.Two);
		deal.PlayerHands[0].AddCard(openingCard);
		deal.PlayerHands[0].AddCard(new Card(Suit.Clubs, Rank.Two));
		MichiganRound round = new MichiganRound(deal, 0);

		bool result = round.TryLeadOpeningCard(0, openingCard);

		Assert.True(result);
		Assert.True(round.IsRunStopped);
		Assert.Equal(0, round.CurrentLeaderIndex);
	}

	[Fact]
	public void TryLeadOpeningCard_WhenLeaderCannotLeadAfterStop_SelectsNextEligiblePlayer()
	{
		Deal deal = CreateEmptyDeal();
		Card openingCard = new Card(Suit.Hearts, Rank.Two);
		deal.PlayerHands[0].AddCard(openingCard);
		deal.PlayerHands[0].AddCard(new Card(Suit.Hearts, Rank.King));
		deal.PlayerHands[1].AddCard(new Card(Suit.Clubs, Rank.Two));
		MichiganRound round = new MichiganRound(deal, 0);

		bool result = round.TryLeadOpeningCard(0, openingCard);

		Assert.True(result);
		Assert.True(round.IsRunStopped);
		Assert.Equal(1, round.CurrentLeaderIndex);
	}

	[Fact]
	public void TryLeadOpeningCard_WhenNoPlayerCanLeadAfterStop_CompletesWithoutWinner()
	{
		Deal deal = CreateEmptyDeal();
		Card openingCard = new Card(Suit.Hearts, Rank.Two);
		deal.PlayerHands[0].AddCard(openingCard);
		deal.PlayerHands[0].AddCard(new Card(Suit.Hearts, Rank.King));
		deal.PlayerHands[1].AddCard(new Card(Suit.Hearts, Rank.Five));
		MichiganRound round = new MichiganRound(deal, 0);

		bool result = round.TryLeadOpeningCard(0, openingCard);

		Assert.True(result);
		Assert.True(round.IsRunStopped);
		Assert.True(round.IsRoundComplete);
		Assert.Null(round.WinnerIndex);
	}

	[Fact]
	public void TryPlayNextCard_WhenPlayedCardEmptiesHand_CompletesRoundWithThatPlayerAsWinner()
	{
		Deal deal = CreateEmptyDeal();
		Card openingCard = new Card(Suit.Hearts, Rank.Two);
		Card nextCard = new Card(Suit.Hearts, Rank.Three);
		deal.PlayerHands[0].AddCard(openingCard);
		deal.PlayerHands[1].AddCard(nextCard);
		MichiganRound round = new MichiganRound(deal, 0);
		Assert.True(round.TryLeadOpeningCard(0, openingCard));

		bool result = round.TryPlayNextCard(1, nextCard);

		Assert.True(result);
		Assert.True(round.IsRoundComplete);
		Assert.Equal(1, round.WinnerIndex);
		Assert.False(round.TryPlayNextCard(0, new Card(Suit.Hearts, Rank.Four)));
	}

	[Fact]
	public void TryPlayNextCard_WhenRunStopsAndLastPlayerCannotLead_SelectsNextEligiblePlayer()
	{
		Deal deal = CreateEmptyDeal();
		Card openingCard = new Card(Suit.Hearts, Rank.Two);
		Card nextCard = new Card(Suit.Hearts, Rank.Three);
		deal.PlayerHands[0].AddCard(openingCard);
		deal.PlayerHands[0].AddCard(new Card(Suit.Clubs, Rank.Two));
		deal.PlayerHands[1].AddCard(nextCard);
		deal.PlayerHands[1].AddCard(new Card(Suit.Hearts, Rank.Nine));
		MichiganRound round = new MichiganRound(deal, 0);
		Assert.True(round.TryLeadOpeningCard(0, openingCard));

		bool result = round.TryPlayNextCard(1, nextCard);

		Assert.True(result);
		Assert.True(round.IsRunStopped);
		Assert.Equal(0, round.CurrentLeaderIndex);
	}

	[Fact]
	public void TryLeadCardAfterStop_WhenValidDifferentSuitLeadContinuesRun()
	{
		Deal deal = CreateEmptyDeal();
		Card openingCard = new Card(Suit.Hearts, Rank.Two);
		Card nextLead = new Card(Suit.Clubs, Rank.Two);
		deal.PlayerHands[0].AddCard(openingCard);
		deal.PlayerHands[0].AddCard(new Card(Suit.Hearts, Rank.King));
		deal.PlayerHands[0].AddCard(nextLead);
		deal.PlayerHands[1].AddCard(new Card(Suit.Clubs, Rank.Three));
		MichiganRound round = new MichiganRound(deal, 0);
		Assert.True(round.TryLeadOpeningCard(0, openingCard));
		Assert.True(round.IsRunStopped);

		bool result = round.TryLeadCardAfterStop(0, nextLead);

		Assert.True(result);
		Assert.False(round.IsRunStopped);
		Assert.Equal(nextLead, round.LastPlayedCard);
	}

	[Fact]
	public void TryLeadCardAfterStop_WhenLeadEmptiesHand_CompletesRoundWithLeaderAsWinner()
	{
		Deal deal = CreateEmptyDeal();
		Card openingCard = new Card(Suit.Hearts, Rank.Two);
		Card nextCard = new Card(Suit.Hearts, Rank.Three);
		Card nextLead = new Card(Suit.Clubs, Rank.Two);
		deal.PlayerHands[0].AddCard(openingCard);
		deal.PlayerHands[0].AddCard(nextLead);
		deal.PlayerHands[1].AddCard(nextCard);
		deal.PlayerHands[1].AddCard(new Card(Suit.Hearts, Rank.Nine));
		MichiganRound round = new MichiganRound(deal, 0);
		Assert.True(round.TryLeadOpeningCard(0, openingCard));
		Assert.True(round.TryPlayNextCard(1, nextCard));
		Assert.True(round.IsRunStopped);
		Assert.Equal(0, round.CurrentLeaderIndex);

		bool result = round.TryLeadCardAfterStop(0, nextLead);

		Assert.True(result);
		Assert.True(round.IsRoundComplete);
		Assert.Equal(0, round.WinnerIndex);
	}

	[Fact]
	public void TryLeadCardAfterStop_WhenPlayerIsNotLeaderOrCardIsInvalid_ReturnsFalseWithoutChangingState()
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
		Assert.True(round.TryLeadOpeningCard(0, openingCard));
		Assert.True(round.IsRunStopped);

		Assert.False(round.TryLeadCardAfterStop(1, otherPlayerCard));
		Assert.False(round.TryLeadCardAfterStop(0, sameSuitCard));
		Assert.False(round.TryLeadCardAfterStop(0, higherClub));
		Assert.True(round.IsRunStopped);
		Assert.Equal(openingCard, round.LastPlayedCard);
		Assert.True(deal.PlayerHands[0].Contains(sameSuitCard));
		Assert.True(deal.PlayerHands[0].Contains(lowestClub));
		Assert.True(deal.PlayerHands[0].Contains(higherClub));
		Assert.True(deal.PlayerHands[1].Contains(otherPlayerCard));
	}

	private static Deal CreateEmptyDeal()
	{
		Deal deal = new Deal(new Deck(), 2);

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