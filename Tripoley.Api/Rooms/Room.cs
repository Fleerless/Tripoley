using Tripoley.Engine;

namespace Tripoley.Api.Rooms;

public sealed class Room(string code)
{
    public string Code { get; } = code;
    public RoomState State { get; private set; } = RoomState.Lobby;
    public Game? Game { get; private set; }

    private readonly List<RoomMember> _members = [];
    private readonly object _lock = new();

    public RoomMember? AddMember(string connectionId, string name)
    {
        lock (_lock)
        {
            if (State != RoomState.Lobby) return null;
            if (_members.Count >= Game.MaxPlayerCount) return null;

            var member = new RoomMember(new Player(Game.StartingChips, name), connectionId);
            _members.Add(member);
            return member;
        }
    }

    public bool RemoveMember(string connectionId)
    {
        lock (_lock)
        {
            if (State != RoomState.Lobby) return false;
            return _members.RemoveAll(member => member.ConnectionId == connectionId) > 0;
        }
    }

    public bool SetReady(string connectionId, bool isReady)
    {
        lock (_lock)
        {
            var member = _members.FirstOrDefault(member => member.ConnectionId == connectionId);
            if (member is null) return false;

            member.IsReady = isReady;
            return true;
        }
    }

    public bool StartGame()
    {
        lock (_lock)
        {
            if (State != RoomState.Lobby) return false;
            if (_members.Count < Game.MinPlayerCount) return false;
            if (!_members.All(member => member.IsReady)) return false;

            Game = new Game(_members.Select(member => member.Player));
            Game.StartNewDeal();
            // Will need to be changed when Hearts/Poker rounds are implemented
            Game.StartMichiganRound();
            State = RoomState.InProgress;
            return true;
        }
    }

    public RoomMember? FindMember(string connectionId)
    {
        lock (_lock)
        {
            return _members.FirstOrDefault(member => member.ConnectionId == connectionId);
        }
    }
    public IReadOnlyList<RoomMember> Members()
    {
        lock (_lock)
        {
            return _members.ToList();
        }
    }

    public bool TryPlayCard(string connectionId, Card card)
    {
        lock (_lock)
        {
            if (State != RoomState.InProgress || Game?.CurrentDeal is null) return false;

            var member = _members.FirstOrDefault(member => member.ConnectionId == connectionId);
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
}
