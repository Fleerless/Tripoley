namespace Tripoley.Engine;

public record Seat(Player Player, Hand Hand);

public class Deal
{
    private readonly List<Seat> _seats;
    public IReadOnlyList<Seat> Seats => _seats;
    public Hand Widow { get; }

    private Deal(IReadOnlyList<Player> playersInDealOrder)
    {
        Widow = new Hand();
        _seats = playersInDealOrder
            .Select(player => new Seat(player, new Hand()))
            .ToList();
    }

    private void DealCards(Deck deck)
    {
        int slotCount = _seats.Count + 1;
        for (int slot = 0; deck.CardsRemaining > 0; slot = (slot + 1) % slotCount)
        {
            Hand target = slot == _seats.Count ? Widow : _seats[slot].Hand;
            target.AddCard(deck.Draw());
        }
    }

    public static Deal Create(Deck deck, IReadOnlyList<Player> playersInDealOrder)
    {
        ArgumentNullException.ThrowIfNull(deck);
        ArgumentNullException.ThrowIfNull(playersInDealOrder);

        if (playersInDealOrder.Count < Game.MinPlayerCount ||
            playersInDealOrder.Count > Game.MaxPlayerCount)
        {
            throw new ArgumentException("Invalid player count.", nameof(playersInDealOrder));
        }

        if (deck.CardsRemaining == 0)
        {
            throw new InvalidOperationException("Cannot create a deal from an empty deck.");
        }

        var deal = new Deal(playersInDealOrder);
        deal.DealCards(deck);
        return deal;
    }
}