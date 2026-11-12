import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import type { RoomMemberView, RoomView } from "../api/contracts";
import type { Game } from "../useGame";
import { Lobby } from "./Lobby";

function member(name: string, isReady: boolean): RoomMemberView {
  return { playerId: name, name, isReady };
}

function makeGame(overrides: Partial<Game> = {}): Game {
  return {
    connected: true,
    room: null,
    gameState: null,
    hand: null,
    openRooms: [],
    error: null,
    createRoom: vi.fn(),
    joinRoom: vi.fn(),
    setReady: vi.fn(),
    startGame: vi.fn(),
    playCard: vi.fn(),
    ...overrides,
  };
}

function room(members: RoomMemberView[]): RoomView {
  return { code: "ABCD", state: "Lobby", members };
}

describe("Lobby", () => {
  it("lists the room code and each member's ready status", () => {
    render(<Lobby game={makeGame()} room={room([member("Jeff", true), member("Ann", false)])} />);

    expect(screen.getByText("ABCD")).toBeInTheDocument();
    expect(screen.getByText("Jeff")).toBeInTheDocument();
    expect(screen.getByText(/✓ ready/)).toBeInTheDocument();
    expect(screen.getByText(/waiting/)).toBeInTheDocument();
  });

  it("toggles ready state through the hub", async () => {
    const setReady = vi.fn();
    render(<Lobby game={makeGame({ setReady })} room={room([member("Jeff", false)])} />);

    await userEvent.click(screen.getByRole("button", { name: /^ready$/i }));
    await userEvent.click(screen.getByRole("button", { name: /unready/i }));

    expect(setReady).toHaveBeenNthCalledWith(1, true);
    expect(setReady).toHaveBeenNthCalledWith(2, false);
  });

  it("disables start until at least two players are all ready", () => {
    render(<Lobby game={makeGame()} room={room([member("Jeff", true)])} />);

    expect(screen.getByRole("button", { name: /start game/i })).toBeDisabled();
    expect(screen.getByText(/need at least two players/i)).toBeInTheDocument();
  });

  it("enables start and calls the hub when everyone is ready", async () => {
    const startGame = vi.fn();
    render(
      <Lobby
        game={makeGame({ startGame })}
        room={room([member("Jeff", true), member("Ann", true)])}
      />,
    );

    const start = screen.getByRole("button", { name: /start game/i });
    expect(start).toBeEnabled();
    await userEvent.click(start);

    expect(startGame).toHaveBeenCalledOnce();
  });
});
