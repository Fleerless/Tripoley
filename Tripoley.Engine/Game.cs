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
    private Guid _dealerId;
    public Deal? CurrentDeal { get; private set; }
    public GamePhase GamePhase  { get; private set; } = GamePhase.WaitingForDeal;
    public ChipPools Pools { get; } = new();
    public const int MinPlayerCount = 2;
    public const int MaxPlayerCount = 9;
    public const int StartingChips = 100;
    public MichiganRound? CurrentMichiganRound { get; private set; }
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
            CurrentDeal = null;
            return;
        }

        _dealerId = _dealerId == Guid.Empty 
            ? eligible[0].Id
            : NextEligibleDealer(_dealerId, eligible);

        foreach (Player player in eligible) Pools.AddAnte(player);

        var deck = new Deck();
        deck.Shuffle();

        CurrentDeal = Deal.Create(deck, eligible);
        GamePhase = GamePhase.DealComplete;
    }
    public void StartMichiganRound()
    {
        StartMichiganRound(FindLeaderPosition(_dealerId, CurrentDeal!.Seats));
    }
    public void StartMichiganRound(int firstLeaderIndex)
    {
        if (CurrentDeal == null || GamePhase != GamePhase.DealComplete) throw new InvalidOperationException("Cannot start Michigan round before deal is complete.");

        CurrentMichiganRound = new MichiganRound(CurrentDeal, firstLeaderIndex);
        GamePhase = GamePhase.MichiganRummyInProgress;
    }

    public void SettleMichiganRound()
    {
        if (CurrentMichiganRound!.WinnerIndex is int winnerIndex)
        {
            var winner = CurrentDeal!.Seats[winnerIndex].Player;

            Pools.ClaimChips(ChipPoolType.Kitty, winner);

            for (int i = 0; i < CurrentDeal.Seats.Count; i ++)
            {
                if (i == winnerIndex) continue;

                Player player = CurrentDeal.Seats[i].Player;
                int remainingCards = CurrentDeal.Seats[i].Hand.CardsRemaining;
                
                CollectPerCard(player, remainingCards, winner.AddChips);
            }
        }
        else
        {
            ChipPool kitty = Pools.Get(ChipPoolType.Kitty);
            foreach (Seat seat in CurrentDeal!.Seats)
            {
                CollectPerCard(seat.Player, seat.Hand.CardsRemaining, kitty.AddChips);  
            }
        }
    }

    public bool TryPlayCard(int playerIndex, Card card)
    {
        if (CurrentMichiganRound == null || GamePhase != GamePhase.MichiganRummyInProgress) return false;

        bool played = CurrentMichiganRound.TryPlay(playerIndex, card);
        
        if (played && CurrentMichiganRound.Phase == RoundPhase.RoundComplete)
        {
            SettleMichiganRound();
            GamePhase = GamePhase.WaitingForDeal;
        }
        return played;
    }

    private static void CollectPerCard(Player payer, int cardCount, Action<int> deposit)
    {
        int payment = Math.Min(cardCount, payer.ChipCount);
        payer.TryRemoveChips(payment);
        deposit(payment);
    }

    private Guid NextEligibleDealer(Guid currentDealerId, IReadOnlyList<Player> eligible)
    {
        var eligibleIds = eligible.Select(p => p.Id).ToHashSet();
        int start = _seatOrder.IndexOf(currentDealerId);

        for (int offset = 1; offset <= _seatOrder.Count; offset++)
        {
            Guid candidate = _seatOrder[(start + offset) % _seatOrder.Count];
            if (eligibleIds.Contains(candidate)) return candidate;
        }
        throw new InvalidOperationException("No eligible leader found.");
    }

    private static int FindLeaderPosition(Guid dealerId, IReadOnlyList<Seat> seats)
    {
        if (dealerId == Guid.Empty) throw new InvalidOperationException("Dealer not set.");
        
        for (int index = 0; index < seats.Count; index++)
        {
            if (seats[index].Player.Id == dealerId)
                return (index + 1) % seats.Count;
        }
        throw new InvalidOperationException("No eligible leader found.");
    }
}