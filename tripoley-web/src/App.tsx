import { useGame } from "./useGame";
import { Landing } from "./components/Landing";
import { Lobby } from "./components/Lobby";
import { Table } from "./components/Table";

export default function App() {
  const game = useGame();
  const next = game.gameState?.nextCard ?? null;

  if (game.room === null) {
    return (
      <div className="app">
        <div className="sign">Tripoley</div>
        <Landing game={game} />
      </div>
    );
  }

  if (game.room.state === "Lobby") {
    return (
      <div className="app">
        <div className="sign">Tripoley</div>
        <Lobby game={game} room={game.room} />
      </div>
    );
  }

  return (
    <div className="app">
      <div className="sign">Tripoley</div>
      <Table game={game} />
      <div className="statusbar">
        <span>{game.connected ? "connected" : "connecting…"}</span>
        {game.gameState && (
          <span>
            {game.gameState.roundPhase ?? "—"}
            {game.gameState.roundPhase === "RunStopped" && " — run stopped; new suit"}
            {next && ` · next: ${next.rank} of ${next.suit}`}
          </span>
        )}
        {game.gameState?.lastRoundWinner && (
          <span className="status-win">
            {game.gameState.lastRoundWinner === "Draw"
              ? "last hand: draw — no one could play"
              : `last hand won by: ${game.gameState.lastRoundWinner}`}
          </span>
        )}
        {game.error && <span className="status-err">error: {game.error}</span>}
        <span className="status-version">
          v{(import.meta.env.VITE_APP_VERSION as string | undefined) ?? "dev"}
        </span>
      </div>
    </div>
  );
}
