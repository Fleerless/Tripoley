namespace Tripoley.Engine;

public class Deck
{
    private readonly List<Card> _cards = [];
    public int CardsRemaining => _cards.Count;
    public Deck()
    {
        foreach (Suit suit in Enum.GetValues<Suit>())
        {
            foreach (Rank rank in Enum.GetValues<Rank>())
            {
                _cards.Add(new Card(suit, rank));
            }
        }
    }
    public Card Draw()
    {
        if (_cards.Count == 0)
        {
            throw new InvalidOperationException("No cards remaining in the deck.");
        }
        // Read the last card 
        Card card = _cards[^1];
        // Remove that card from the deck
        _cards.RemoveAt(_cards.Count - 1);
        return card;
    }
    public void Shuffle()
    {
        for (int index = _cards.Count -1; index > 0; index--)
        {
            // Fisher-Yates shuffle. With 52 cards, 8.07 x 10^67 possible results.
            int swapIndex = Random.Shared.Next(index + 1);
            (_cards[index], _cards[swapIndex]) = 
                (_cards[swapIndex], _cards[index]);
        }
    }
}