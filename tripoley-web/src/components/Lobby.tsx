import type { Game } from "../useGame";
import type { RoomView } from "../api/contracts";
import { ConnectionStatus } from "./ConnectionStatus";

export function Lobby({ game, room }: { game: Game; room: RoomView }) {
  const allReady = room.members.length > 0 && room.members.every((m) => m.isReady);
  const canStart = room.members.length >= 2 && allReady;

  return (
    <div className="screen">
      <ConnectionStatus connected={game.connected} error={game.error} />

      <div className="panel">
        <h3>
          lobby · <span className="room-code">{room.code}</span>
        </h3>

        <ul className="member-list">
          {room.members.map((m) => (
            <li key={m.playerId} className={m.isReady ? "member ready" : "member"}>
              <span className="member-name">{m.name}</span>
              <span className="member-status">{m.isReady ? "✓ ready" : "waiting…"}</span>
            </li>
          ))}
        </ul>

        <div className="lobby-actions">
          <button className="btn-primary" onClick={() => game.setReady(true)}>
            ready
          </button>
          <button onClick={() => game.setReady(false)}>unready</button>
        </div>

        <button className="btn-start" onClick={() => game.startGame()} disabled={!canStart}>
          start game
        </button>
        {!canStart && (
          <p className="muted">need at least two players, all marked ready</p>
        )}
      </div>
    </div>
  );
}
