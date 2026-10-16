namespace Tripoley.Engine;

public class Deal
{
    public const int MinPlayerCount = 2;
    public const int MaxPlayerCount = 9;
    private readonly List<Hand> _playerHands = [];
    public IReadOnlyList<Hand> PlayerHands => _playerHands;
    public Hand Widow { get; private set; }

    public Deal(int playerCount)
    {
        if (playerCount < MinPlayerCount || playerCount > MaxPlayerCount)
        {
            throw new ArgumentOutOfRangeException(nameof(playerCount), $"Player count must be between {MinPlayerCount} and {MaxPlayerCount}.");
        }

        Widow = new Hand();

        for (int index = 0; index < playerCount; index++)
        {
            _playerHands.Add(new Hand());
        }
    }
    private void DealCards(Deck deck)
    {
        int slotCount = _playerHands.Count + 1;
        for (int slot = 0; deck.CardsRemaining > 0; slot = (slot + 1) % slotCount)
        {
            Hand target = slot == _playerHands.Count ? Widow : _playerHands[slot];
            target.AddCard(deck.Draw());
        }
        
    }
    public static Deal Create(Deck deck, int playerCount)
    {
        ArgumentNullException.ThrowIfNull(deck);

        if (deck.CardsRemaining == 0)
        {
            throw new InvalidOperationException("Cannot create a deal from an empty deck.");
        }

        var deal = new Deal(playerCount);
        deal.DealCards(deck);
        return deal;
    }
}