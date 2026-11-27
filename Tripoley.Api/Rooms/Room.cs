using Tripoley.Engine;

namespace Tripoley.Api.Rooms;

public sealed class Room(string code, Random? random = null, TimeProvider? timeProvider = null)
{
    public string Code { get; } = code;
    public RoomState State { get; private set; } = RoomState.Lobby;
    public Game? Game { get; private set; }

    public DateTimeOffset? EmptySince { get; private set; }

    private readonly List<RoomMember> _members = [];
    private readonly object _lock = new();
    private readonly Random _random = random ?? Random.Shared;
    private readonly TimeProvider _time = timeProvider ?? TimeProvider.System;

    public event Action<string>? MemberAdded;

    public RoomMember? AddMember(string userId, string name)
    {
        RoomMember member;
        lock (_lock)
        {
            if (State != RoomState.Lobby) return null;
            if (_members.Count >= Game.MaxPlayerCount) return null;

            member = new RoomMember(new Player(Game.StartingChips, name), userId);
            _members.Add(member);
            RefreshEmptySince();
        }

        MemberAdded?.Invoke(userId);
        return member;
    }

    public bool SetReady(string userId, bool isReady)
    {
        lock (_lock)
        {
            var member = FindMemberNoLock(userId);
            if (member is null) return false;

            member.IsReady = isReady;
            return true;
        }
    }

    public RoomMember? MarkDisconnected(string userId) => SetConnected(userId, false);

    public RoomMember? MarkConnected(string userId) => SetConnected(userId, true);

    private RoomMember? SetConnected(string userId, bool isConnected)
    {
        lock (_lock)
        {
            var member = FindMemberNoLock(userId);
            if (member is null) return null;

            member.IsConnected = isConnected;
            RefreshEmptySince();
            return member;
        }
    }

    public bool IsAbandonedSince(DateTimeOffset cutoff)
    {
        lock (_lock)
        {
            return EmptySince is { } since && since <= cutoff;
        }
    }

    public bool StartGame()
    {
        lock (_lock)
        {
            if (State != RoomState.Lobby) return false;
            if (_members.Count < Game.MinPlayerCount) return false;
            if (!_members.All(member => member.IsReady && member.IsConnected)) return false;

            Game = new Game(_members.Select(member => member.Player), _random);
            Game.StartNewDeal();
            if (Game.GamePhase == GamePhase.DealComplete)
            {
                // Poker will slot in here between Hearts and Michigan when that phase is implemented.
                Game.StartHeartsPhase();
                Game.StartMichiganRound();
            }
            State = RoomState.InProgress;
            return true;
        }
    }

    public RoomMember? FindMember(string userId)
    {
        lock (_lock)
        {
            return FindMemberNoLock(userId);
        }
    }
    public IReadOnlyList<RoomMember> Members()
    {
        lock (_lock)
        {
            return _members.ToList();
        }
    }

    public bool TryPlayCard(string userId, Card card)
    {
        lock (_lock)
        {
            if (State != RoomState.InProgress || Game?.CurrentDeal is null) return false;

            var member = FindMemberNoLock(userId);
            if (member is null) return false;

            var seats = Game.CurrentDeal.Seats;
            int seatIndex = -1;
            for (int index = 0; index < seats.Count; index++)
            {
                if (seats[index].Player.Id == member.Player.Id)
                {
                    seatIndex = index;
                    break;
                }
            }
            if (seatIndex < 0) return false;

            return Game.TryPlayCard(seatIndex, card);
        }
    }

    public bool AdvanceToNextDeal()
    {
        lock (_lock)
        {
            return AdvanceToNextDealNoLock();
        }
    }

    private bool AdvanceToNextDealNoLock()
    {
        if (State != RoomState.InProgress || Game is null) return false;
        if (Game.GamePhase != GamePhase.WaitingForDeal) return false;

        Game.StartNewDeal();
        if (Game.GamePhase == GamePhase.GameComplete)
        {
            State = RoomState.Completed;
            return true;
        }

        // Poker will slot in here between Hearts and Michigan when that phase is implemented.
        Game.StartHeartsPhase();
        Game.StartMichiganRound();
        return true;
    }

    public bool Leave(string userId)
    {
        lock (_lock)
        {
            var member = FindMemberNoLock(userId);
            if (member is null) return false;

            if (State == RoomState.InProgress && Game is not null && !Game.IsAbandoned(member.Player.Id))
            {
                Game.Abandon(member.Player.Id);
                if (Game.GamePhase == GamePhase.GameComplete)
                    State = RoomState.Completed;
                else if (Game.GamePhase == GamePhase.WaitingForDeal)
                    AdvanceToNextDealNoLock();
            }

            _members.Remove(member);
            RefreshEmptySince();
            return true;
        }
    }

    // Keep `read` CPU-only: no I/O, so the lock is never held across an await.
    public T ReadGame<T>(Func<Game, IReadOnlyList<RoomMember>, T> read)
    {
        lock (_lock)
        {
            if (Game is null) throw new InvalidOperationException("No game in progress.");
            return read(Game, _members);
        }
    }

    private RoomMember? FindMemberNoLock(string userId) =>
        _members.FirstOrDefault(member => member.UserId == userId);

    private void RefreshEmptySince()
    {
        bool allAway = _members.Count > 0 && _members.All(member => !member.IsConnected);
        EmptySince = allAway ? EmptySince ?? _time.GetUtcNow() : null;
    }
}
