import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, describe, expect, it, vi } from "vitest";
import type { CardDto, GameStateView, PlayerSeatView, SelfView } from "../api/contracts";
import type { Game } from "../useGame";
import { Table } from "./Table";

function setViewport(isMobile: boolean) {
  window.matchMedia = vi.fn().mockImplementation((query: string) => ({
    matches: isMobile,
    media: query,
    onchange: null,
    addEventListener: () => {},
    removeEventListener: () => {},
    addListener: () => {},
    removeListener: () => {},
    dispatchEvent: () => false,
  })) as unknown as typeof window.matchMedia;
}

const seats: PlayerSeatView[] = [
  { seatIndex: 0, name: "Alice", chipCount: 9, cardsRemaining: 5 },
  { seatIndex: 1, name: "Bob", chipCount: 9, cardsRemaining: 2 },
];

const myHand: CardDto[] = [
  { suit: "Clubs", rank: "Three" },
  { suit: "Hearts", rank: "King" },
];

function makeGame(overrides: Partial<Game> = {}): Game {
  const gameState: GameStateView = {
    phase: "MichiganRummyInProgress",
    roundPhase: "RunInProgress",
    currentLeaderIndex: 1,
    currentTurnSeatIndex: 1,
    lastPlayedCard: { suit: "Spades", rank: "Ace" },
    nextCard: null,
    winnerIndex: null,
    lastRoundWinner: null,
    pools: [{ type: "Kitty", chipCount: 7 }],
    seats,
  };
  const hand: SelfView = { seatIndex: 1, hand: myHand, legalPlays: [myHand[0]] };
  return {
    connected: true,
    room: null,
    gameState,
    hand,
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

afterEach(() => vi.restoreAllMocks());

describe("Table (mobile)", () => {
  it("shows opponents in the strip and the self card, excluding self from opponents", () => {
    setViewport(true);
    render(<Table game={makeGame()} />);
    expect(screen.getByText("Alice")).toBeInTheDocument();
    expect(screen.getByText(/\(you\)/)).toBeInTheDocument();
  });

  it("plays a card when it is tapped", async () => {
    setViewport(true);
    const playCard = vi.fn();
    render(<Table game={makeGame({ playCard })} />);
    await userEvent.click(screen.getByText("3"));
    expect(playCard).toHaveBeenCalledWith({ suit: "Clubs", rank: "Three" });
  });

  it("renders without a self row when the viewer holds no seat", () => {
    setViewport(true);
    const hand: SelfView = { seatIndex: null, hand: [], legalPlays: [] };
    render(<Table game={makeGame({ hand })} />);
    expect(screen.getByText("Alice")).toBeInTheDocument();
    expect(screen.getByText("Bob")).toBeInTheDocument();
    expect(screen.queryByText(/\(you\)/)).not.toBeInTheDocument();
  });
});

describe("Table (desktop)", () => {
  it("renders the desktop mat with every seat", () => {
    setViewport(false);
    render(<Table game={makeGame()} />);
    expect(screen.getByText("Alice")).toBeInTheDocument();
    expect(screen.getByText(/Bob/)).toBeInTheDocument();
  });

  it("plays the self seat's card when clicked", async () => {
    setViewport(false);
    const playCard = vi.fn();
    render(<Table game={makeGame({ playCard })} />);
    expect(screen.getByText(/\(you\)/)).toBeInTheDocument();
    await userEvent.click(screen.getByText("3"));
    expect(playCard).toHaveBeenCalledWith({ suit: "Clubs", rank: "Three" });
  });

  it("renders safely with no game state or hand", () => {
    setViewport(false);
    render(<Table game={makeGame({ gameState: null, hand: null })} />);
    expect(screen.queryByText("Alice")).not.toBeInTheDocument();
  });
});
