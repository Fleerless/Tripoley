import { useState } from "react";
import { useGame } from "./useGame";
import { Table } from "./components/Table";

const outlineIfError = (hasError: boolean) =>
  hasError ? ({ outline: "2px solid crimson" } as const) : undefined;

export default function App() {
  const game = useGame();
  const [name, setName] = useState("");
  const [code, setCode] = useState("");
  const [fieldError, setFieldError] = useState<"name" | "code" | null>(null);

  const next = game.gameState?.nextCard ?? null;

  const hasName = () => {
    if (name.trim().length === 0) {
      setFieldError("name");
      return false;
    }
    return true;
  };

  const createRoom = () => {
    if (!hasName()) return;
    setFieldError(null);
    game.createRoom(name);
  };

  const joinByCode = () => {
    if (!hasName()) return;
    if (code.trim().length === 0) {
      setFieldError("code");
      return;
    }
    setFieldError(null);
    game.joinRoom(code, name);
  };

  const joinListedRoom = (roomCode: string) => {
    if (!hasName()) return;
    setFieldError(null);
    game.joinRoom(roomCode, name);
  };

  const header = (
    <>
      <p>connection: {game.connected ? "connected" : "connecting…"}</p>
      {game.error && <p style={{ color: "crimson" }}>error: {game.error}</p>}
    </>
  );

  if (game.room === null) {
    return (
      <div className="app">
      <div className="sign">Tripoley</div>
        {header}
        <div>
          your name:{" "}
          <input
            value={name}
            onChange={(e) => {
              setName(e.target.value);
              if (fieldError === "name") setFieldError(null);
            }}
            style={outlineIfError(fieldError === "name")}
          />
          {fieldError === "name" && (
            <span style={{ color: "crimson", marginLeft: 8 }}>enter a name first</span>
          )}
        </div>

        <div style={{ marginTop: 8 }}>
          <button onClick={createRoom}>create room</button>
          <span style={{ margin: "0 8px" }}>or join by code:</span>
          <input
            value={code}
            onChange={(e) => {
              setCode(e.target.value);
              if (fieldError === "code") setFieldError(null);
            }}
            style={outlineIfError(fieldError === "code")}
          />
          <button onClick={joinByCode}>join</button>
          {fieldError === "code" && (
            <span style={{ color: "crimson", marginLeft: 8 }}>enter a room code</span>
          )}
        </div>

        <h3>open rooms</h3>
        {game.openRooms.length === 0 && <p>(no open rooms)</p>}
        {game.openRooms.map((room) => (
          <button
            key={room.code}
            onClick={() => joinListedRoom(room.code)}
            style={{ display: "block", marginBottom: 4 }}
          >
            join {room.code} ({room.playerCount}/{room.capacity})
          </button>
        ))}
      </div>
    );
  }

  if (game.room.state === "Lobby") {
    return (
      <div className="app">
      <div className="sign">Tripoley</div>
        {header}
        <h3>lobby · {game.room.code}</h3>
        <ul>
          {game.room.members.map((m) => (
            <li key={m.playerId}>
              {m.name} {m.isReady ? "✓ ready" : "…"}
            </li>
          ))}
        </ul>
        <button onClick={() => game.setReady(true)}>ready</button>
        <button onClick={() => game.setReady(false)}>unready</button>
        <button onClick={() => game.startGame()}>start game</button>
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