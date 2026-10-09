namespace Tripoley.Engine;

public class Deal
{
    private readonly List<Hand> _playerHands = [];
    public IReadOnlyList<Hand> PlayerHands => _playerHands;
    public Hand Widow { get; }

    public Deal(Deck deck, int playerCount)
    {
        ArgumentNullException.ThrowIfNull(deck);

        if (deck.CardsRemaining == 0)
        {
            throw new InvalidOperationException("Cannot deal with an empty deck.");
        }

        if (playerCount < 2 || playerCount > 9)
        {
            throw new ArgumentOutOfRangeException(nameof(playerCount), "Player count must be between 2 and 9.");
        }

        Widow = new Hand();

        for (int index = 0; index < playerCount; index++)
        {
            _playerHands.Add(new Hand());
        }

        while (deck.CardsRemaining > 0)
        {
            for (
                int playerIndex = 0;
                playerIndex < _playerHands.Count && deck.CardsRemaining > 0;
                playerIndex++)
            {
                _playerHands[playerIndex].AddCard(deck.Draw());
            }
            if (deck.CardsRemaining > 0)
            {
                Widow.AddCard(deck.Draw());
            }
        }
    }
}