namespace Tripoley.Engine;

public enum GamePhase
{
    WaitingForDeal,
    DealComplete,
    HeartsInProgress,
    PokerInProgress,
    MichiganRummyInProgress,
    GameComplete
}

public class Game
{
    private readonly Dictionary<Guid, Player> _players;
    private readonly List<Guid> _seatOrder;
    public Deal? CurrentDeal { get; private set; }
    public GamePhase GamePhase  { get; private set; } = GamePhase.WaitingForDeal;
    public ChipPools Pools { get; } = new();
    public const int MinPlayerCount = 2;
    public const int MaxPlayerCount = 9;
    public Game(IEnumerable<Player> players)
    {
        ArgumentNullException.ThrowIfNull(players);

        _players = new Dictionary<Guid, Player>();
        _seatOrder = new List<Guid>();

        foreach (Player player in players)
        {
            if (player is null) throw new ArgumentException("Players must not contain null entries.", nameof(players));
            if (!_players.TryAdd(player.Id, player)) throw new ArgumentException($"Duplicate player ID: {player.Id}.", nameof(players));
            
            _players[player.Id] = player;
            _seatOrder.Add(player.Id);
        }

        if (_players.Count < MinPlayerCount || _players.Count > MaxPlayerCount) throw new ArgumentException($"Player count must be between {MinPlayerCount} and {MaxPlayerCount}.", nameof(players));
    }
    public void StartNewDeal()
    {
        if (GamePhase != GamePhase.WaitingForDeal) return;

        var eligible = _seatOrder
            .Select(id => _players[id])
            .Where(p => p.ChipCount >= Pools.AnteCost)
            .ToList();

        if (eligible.Count < MinPlayerCount)
        {
            GamePhase = GamePhase.GameComplete;
            return;
        }

        foreach (Player player in eligible) Pools.AddAnte(player);

        var deck = new Deck();
        deck.Shuffle();

        CurrentDeal = Deal.Create(deck, eligible);
        GamePhase = GamePhase.DealComplete;

    }
}