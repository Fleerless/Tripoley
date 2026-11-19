import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import type { OpenRoomView } from "../api/contracts";
import type { Game } from "../useGame";
import { Landing } from "./Landing";

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

describe("Landing", () => {
  it("creates a room with the entered name", async () => {
    const createRoom = vi.fn();
    render(<Landing game={makeGame({ createRoom })} />);

    await userEvent.type(screen.getByRole("textbox", { name: /your name/i }), "Jeff");
    await userEvent.click(screen.getByRole("button", { name: /create room/i }));

    expect(createRoom).toHaveBeenCalledWith("Jeff");
  });

  it("blocks creating a room without a name and shows a hint", async () => {
    const createRoom = vi.fn();
    render(<Landing game={makeGame({ createRoom })} />);

    await userEvent.click(screen.getByRole("button", { name: /create room/i }));

    expect(createRoom).not.toHaveBeenCalled();
    expect(screen.getByText(/enter a name first/i)).toBeInTheDocument();
  });

  it("joins by code when both a name and a code are given", async () => {
    const joinRoom = vi.fn();
    render(<Landing game={makeGame({ joinRoom })} />);

    await userEvent.type(screen.getByRole("textbox", { name: /your name/i }), "Jeff");
    await userEvent.type(screen.getByRole("textbox", { name: /room code/i }), "ABCD");
    await userEvent.click(screen.getByRole("button", { name: /^join$/i }));

    expect(joinRoom).toHaveBeenCalledWith("ABCD", "Jeff");
  });

  it("requires a code when joining by code", async () => {
    const joinRoom = vi.fn();
    render(<Landing game={makeGame({ joinRoom })} />);

    await userEvent.type(screen.getByRole("textbox", { name: /your name/i }), "Jeff");
    await userEvent.click(screen.getByRole("button", { name: /^join$/i }));

    expect(joinRoom).not.toHaveBeenCalled();
    expect(screen.getByText(/enter a room code/i)).toBeInTheDocument();
  });

  it("joins a listed open room, carrying the entered name", async () => {
    const joinRoom = vi.fn();
    const openRooms: OpenRoomView[] = [{ code: "WXYZ", playerCount: 1, capacity: 9 }];
    render(<Landing game={makeGame({ joinRoom, openRooms })} />);

    await userEvent.type(screen.getByRole("textbox", { name: /your name/i }), "Jeff");
    await userEvent.click(screen.getByRole("button", { name: /WXYZ/ }));

    expect(joinRoom).toHaveBeenCalledWith("WXYZ", "Jeff");
  });

  it("will not join a listed room without a name", async () => {
    const joinRoom = vi.fn();
    const openRooms: OpenRoomView[] = [{ code: "WXYZ", playerCount: 1, capacity: 9 }];
    render(<Landing game={makeGame({ joinRoom, openRooms })} />);

    await userEvent.click(screen.getByRole("button", { name: /WXYZ/ }));

    expect(joinRoom).not.toHaveBeenCalled();
    expect(screen.getByText(/enter a name first/i)).toBeInTheDocument();
  });

  it("shows a placeholder when no rooms are open and surfaces connection errors", () => {
    render(<Landing game={makeGame({ connected: false, error: "no server" })} />);

    expect(screen.getByText(/no open rooms/i)).toBeInTheDocument();
    expect(screen.getByText("no server")).toBeInTheDocument();
    expect(screen.getByText(/connecting/i)).toBeInTheDocument();
  });
});
