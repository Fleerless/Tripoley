import { useState } from "react";
import type { Game } from "../useGame";
import { ConnectionStatus } from "./ConnectionStatus";

const outlineIfError = (hasError: boolean) =>
  hasError ? ({ outline: "2px solid crimson" } as const) : undefined;

export function Landing({ game }: { game: Game }) {
  const [name, setName] = useState("");
  const [code, setCode] = useState("");
  const [fieldError, setFieldError] = useState<"name" | "code" | null>(null);

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

  return (
    <div className="screen">
      <ConnectionStatus connected={game.connected} error={game.error} />

      <div className="panel">
        <label className="field">
          <span className="field-label">your name</span>
          <input
            value={name}
            onChange={(e) => {
              setName(e.target.value);
              if (fieldError === "name") setFieldError(null);
            }}
            style={outlineIfError(fieldError === "name")}
          />
          {fieldError === "name" && <span className="field-error">enter a name first</span>}
        </label>

        <button className="btn-primary" onClick={createRoom}>
          create room
        </button>

        <div className="divider">or join by code</div>

        <div className="field">
          <div className="join-row">
            <input
              aria-label="room code"
              value={code}
              onChange={(e) => {
                setCode(e.target.value);
                if (fieldError === "code") setFieldError(null);
              }}
              style={outlineIfError(fieldError === "code")}
            />
            <button onClick={joinByCode}>join</button>
          </div>
          {fieldError === "code" && <span className="field-error">enter a room code</span>}
        </div>
      </div>

      <div className="panel">
        <h3>open rooms</h3>
        {game.openRooms.length === 0 && <p className="muted">(no open rooms)</p>}
        <div className="room-list">
          {game.openRooms.map((room) => (
            <button key={room.code} className="room-button" onClick={() => joinListedRoom(room.code)}>
              <span className="room-code">{room.code}</span>
              <span className="room-count">
                {room.playerCount}/{room.capacity}
              </span>
            </button>
          ))}
        </div>
      </div>
    </div>
  );
}
