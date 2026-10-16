namespace Tripoley.Engine;

public sealed class Hand
{
    private readonly List<Card> _cards = [];
    public IReadOnlyList<Card> Cards => _cards.AsReadOnly();
    public int CardsRemaining => _cards.Count;

    internal void AddCard(Card card)
    {
        _cards.Add(card);
    }
    internal bool TryRemoveCard(Card card)
    {
        return _cards.Remove(card);
    }
    public bool Contains(Card card)
    {
        return _cards.Contains(card);
    }
    public bool HasCardOfSuit(Suit suit)
    {
        return _cards.Any(card => card.Suit == suit);
    }
    public bool IsLowestCardOfSuit(Card card)
    {
        if (!Contains(card))
        {
            return false;
        }

        var lowestCard = _cards
            .Where(handCard => handCard.Suit == card.Suit)
            .MinBy(handCard => handCard.Rank);

        return card == lowestCard;
    }
}