import type { Game } from "../useGame";
import type { RoomView } from "../api/contracts";
import { ConnectionStatus } from "./ConnectionStatus";

export function Lobby({ game, room }: { game: Game; room: RoomView }) {
  const everyoneReadyAndPresent =
    room.members.length > 0 && room.members.every((member) => member.isReady && member.isConnected);
  const canStart = room.members.length >= 2 && everyoneReadyAndPresent;

  return (
    <div className="screen">
      <ConnectionStatus connected={game.connected} error={game.error} />

      <div className="panel">
        <h3>
          lobby · <span className="room-code">{room.code}</span>
        </h3>

        <ul className="member-list">
          {room.members.map((member) => (
            <li
              key={member.playerId}
              className={`member${member.isReady ? " ready" : ""}${member.isConnected ? "" : " away"}`}
            >
              <span className="member-name">
                <span className={`conn-dot${member.isConnected ? " online" : ""}`} />
                {member.name}
              </span>
              <span className="member-status">
                {!member.isConnected ? "away" : member.isReady ? "✓ ready" : "waiting…"}
              </span>
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
