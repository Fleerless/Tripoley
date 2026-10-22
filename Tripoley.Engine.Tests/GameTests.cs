namespace Tripoley.Engine.Tests;

public class GameTests
{
    [Fact]
    public void Constructor_NullPlayers_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Game(null!));
    }
    [Fact]
    public void Constructor_NullPlayerEntry_ThrowsArgumentException()
    {
        Player[] players = [new Player(chipCount: 100, name: "Jeff"), null!];

        Assert.Throws<ArgumentException>(() => new Game(players));
    }
    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    public void Constructor_InvalidPlayerCount_ThrowsArgumentException(int playerCount)
    {
        Assert.ThrowsAny<ArgumentException>(() => new Game(CreatePlayers(playerCount)));
    }
    [Fact]
    public void Constructor_ValidPlayers_StartsWaitingForDeal()
    {
        Game game = new Game(CreatePlayers(3));

        Assert.Equal(GamePhase.WaitingForDeal, game.GamePhase);
        Assert.Null(game.CurrentDeal);
        Assert.NotNull(game.Pools);
    }
    [Fact]
    public void StartNewDeal_CollectsAnteFromEveryPlayerIntoEveryPool()
    {
        Player[] players = CreatePlayers(3);
        Game game = new Game(players);

        game.StartNewDeal();

        Assert.All(players, player => Assert.Equal(91, player.ChipCount));
        Assert.All(
            Enum.GetValues<ChipPoolType>(),
            type => Assert.Equal(3, game.Pools.Get(type).ChipCount));
    }
    [Fact]
    public void StartNewDeal_CreatesDealForAllEligiblePlayers()
    {
        Player[] players = CreatePlayers(4);
        Game game = new Game(players);

        game.StartNewDeal();

        Assert.Equal(GamePhase.DealComplete, game.GamePhase);
        Assert.NotNull(game.CurrentDeal);
        Assert.Equal(players, game.CurrentDeal.Seats.Select(seat => seat.Player));
    }
    [Fact]
    public void StartNewDeal_PlayerWithExactlyTheAnteCost_IsEligible()
    {
        Player[] players = [new Player(chipCount: 9, name: "Jeff"), new Player(chipCount: 9, name: "Ann")];
        Game game = new Game(players);

        game.StartNewDeal();

        Assert.Equal(GamePhase.DealComplete, game.GamePhase);
        Assert.All(players, player => Assert.Equal(0, player.ChipCount));
    }
    [Fact]
    public void StartNewDeal_ExcludesPlayersWhoCannotAffordTheAnte()
    {
        Player[] players =
        [
            new Player(chipCount: 100, name: "Jeff"),
            new Player(chipCount: 5, name: "Broke Betty"),
            new Player(chipCount: 100, name: "Ann"),
        ];
        Game game = new Game(players);

        game.StartNewDeal();

        Assert.Equal(GamePhase.DealComplete, game.GamePhase);
        Assert.NotNull(game.CurrentDeal);
        Assert.Equal(2, game.CurrentDeal.Seats.Count);
        Assert.DoesNotContain(players[1], game.CurrentDeal.Seats.Select(seat => seat.Player));
        Assert.Equal(5, players[1].ChipCount);
        Assert.Equal(2 * 9, TotalPoolChips(game));
    }
    [Fact]
    public void StartNewDeal_WhenFewerThanTwoPlayersCanAnte_EndsGameWithoutDealing()
    {
        Player[] players =
        [
            new Player(chipCount: 100, name: "Jeff"),
            new Player(chipCount: 5, name: "Broke Betty"),
            new Player(chipCount: 3, name: "Broke Bob"),
        ];
        Game game = new Game(players);

        game.StartNewDeal();

        Assert.Equal(GamePhase.GameComplete, game.GamePhase);
        Assert.Null(game.CurrentDeal);
        Assert.Equal(100, players[0].ChipCount);
        Assert.Equal(0, TotalPoolChips(game));
    }
    [Fact]
    public void StartNewDeal_WhenNotWaitingForDeal_DoesNothing()
    {
        Player[] players = CreatePlayers(2);
        Game game = new Game(players);
        game.StartNewDeal();

        game.StartNewDeal();

        Assert.Equal(GamePhase.DealComplete, game.GamePhase);
        Assert.All(players, player => Assert.Equal(91, player.ChipCount));
        Assert.Equal(18, TotalPoolChips(game));
    }

    private static Player[] CreatePlayers(int count)
    {
        return Enumerable
            .Range(1, count)
            .Select(index => new Player(chipCount: 100, name: $"Player {index}"))
            .ToArray();
    }

    private static int TotalPoolChips(Game game)
    {
        return Enum.GetValues<ChipPoolType>().Sum(type => game.Pools.Get(type).ChipCount);
    }
}
