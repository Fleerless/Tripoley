namespace Tripoley.Engine;

public class Hand
{
    private readonly List<Card> _cards = [];
    public int CardsRemaining => _cards.Count;

    public void AddCard(Card card)
    {
        _cards.Add(card);
    }
    public bool Contains(Card card)
    {
        return _cards.Contains(card);
    }
}