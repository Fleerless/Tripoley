namespace Tripoley.Engine;

public class Hand
{
    private readonly List<Card> _cards = [];
    public int CardsRemaining => _cards.Count;

    public void AddCard(Card card)
    {
        _cards.Add(card);
    }
    public bool TryRemoveCard(Card card)
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
            .MinBy(handCard => RankExtensions.GetValue(handCard.Rank));

        return card == lowestCard;
    }
}