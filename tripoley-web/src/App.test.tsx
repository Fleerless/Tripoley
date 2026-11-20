import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import type { GameStateView, PlayerSeatView, RoomView, SelfView } from "./api/contracts";
import type { Game } from "./useGame";

const hoisted = vi.hoisted(() => ({ game: null as unknown as Game }));
vi.mock("./useGame", () => ({ useGame: () => hoisted.game }));

import App from "./App";

function makeGame(): Game {
  const seats: PlayerSeatView[] = [
    { seatIndex: 0, name: "Alice", chipCount: 9, cardsRemaining: 3, presence: "Active" },
    { seatIndex: 1, name: "Bob", chipCount: 9, cardsRemaining: 3, presence: "Active" },
  ];
  const gameState: GameStateView = {
    phase: "MichiganRummyInProgress",
    roundPhase: "RunInProgress",
    currentLeaderIndex: 0,
    currentTurnSeatIndex: 0,
    lastPlayedCard: null,
    nextCard: null,
    winnerIndex: null,
    pools: [],
    seats,
  };
  const room: RoomView = { code: "ABCD", state: "InProgress", members: [] };
  const hand: SelfView = { seatIndex: 0, hand: [], legalPlays: [] };
  return {
    connected: true,
    room,
    gameState,
    hand,
    openRooms: [],
    error: null,
    createRoom: vi.fn(),
    joinRoom: vi.fn(),
    setReady: vi.fn(),
    startGame: vi.fn(),
    playCard: vi.fn(),
    leaveRoom: vi.fn(),
  };
}

beforeEach(() => {
  window.matchMedia = vi.fn().mockImplementation((query: string) => ({
    matches: false,
    media: query,
    onchange: null,
    addEventListener: () => {},
    removeEventListener: () => {},
    addListener: () => {},
    removeListener: () => {},
    dispatchEvent: () => false,
  })) as unknown as typeof window.matchMedia;
});

afterEach(() => vi.restoreAllMocks());

describe("App leave control", () => {
  it("asks the server to leave after the player confirms", async () => {
    vi.spyOn(window, "confirm").mockReturnValue(true);
    hoisted.game = makeGame();

    render(<App />);
    await userEvent.click(screen.getByRole("button", { name: /^leave$/i }));

    expect(hoisted.game.leaveRoom).toHaveBeenCalledOnce();
  });

  it("does not leave when the player cancels the confirm", async () => {
    vi.spyOn(window, "confirm").mockReturnValue(false);
    hoisted.game = makeGame();

    render(<App />);
    await userEvent.click(screen.getByRole("button", { name: /^leave$/i }));

    expect(hoisted.game.leaveRoom).not.toHaveBeenCalled();
  });
});
