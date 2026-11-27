import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import type { HeartsAwardView, PlayerSeatView } from "../api/contracts";
import { HeartsReveal } from "./HeartsReveal";

const seats: PlayerSeatView[] = [
  { seatIndex: 0, name: "Alice", chipCount: 9, cardsRemaining: 5, presence: "Active" },
  { seatIndex: 1, name: "Bob", chipCount: 9, cardsRemaining: 5, presence: "Active" },
];

// Seat 1 wins King, Queen and the King-Queen combo; seat 0 wins the Ace and the 8-9-10 run.
const awards: HeartsAwardView[] = [
  { pool: "AceOfHearts", seatIndex: 0, chips: 2, cards: [{ suit: "Hearts", rank: "Ace" }] },
  { pool: "KingOfHearts", seatIndex: 1, chips: 2, cards: [{ suit: "Hearts", rank: "King" }] },
  { pool: "QueenOfHearts", seatIndex: 1, chips: 2, cards: [{ suit: "Hearts", rank: "Queen" }] },
  {
    pool: "KingQueenOfHearts",
    seatIndex: 1,
    chips: 2,
    cards: [
      { suit: "Hearts", rank: "King" },
      { suit: "Hearts", rank: "Queen" },
    ],
  },
  {
    pool: "EightNineTen",
    seatIndex: 0,
    chips: 1,
    cards: [
      { suit: "Clubs", rank: "Eight" },
      { suit: "Clubs", rank: "Nine" },
      { suit: "Clubs", rank: "Ten" },
    ],
  },
];

describe("HeartsReveal", () => {
  it("sums each seat's pools and shows the King-Queen cards only once (mat)", () => {
    render(
      <HeartsReveal awards={awards} seats={seats} mySeatIndex={0} variant="mat" onSkip={vi.fn()} />,
    );

    // Seat 1 took King (2) + Queen (2) + King-Queen (2) = 6; seat 0 took Ace (2) + 8-9-10 (1) = 3.
    expect(screen.getAllByText("+6").length).toBeGreaterThan(0);
    expect(screen.getAllByText("+3").length).toBeGreaterThan(0);
    // The King appears once despite being credited across two pools.
    expect(screen.getAllByText("K")).toHaveLength(1);
    expect(screen.getByText("8")).toBeInTheDocument();
  });

  it("calls onSkip from the mat skip control", async () => {
    const onSkip = vi.fn();
    render(
      <HeartsReveal awards={awards} seats={seats} mySeatIndex={0} variant="mat" onSkip={onSkip} />,
    );

    await userEvent.click(screen.getByText("click to skip"));
    expect(onSkip).toHaveBeenCalled();
  });

  it("lists payouts by name and skips on tap (mobile)", async () => {
    const onSkip = vi.fn();
    render(
      <HeartsReveal
        awards={awards}
        seats={seats}
        mySeatIndex={0}
        variant="mobile"
        onSkip={onSkip}
      />,
    );

    expect(screen.getByText("Hearts payouts")).toBeInTheDocument();
    expect(screen.getByText("Alice")).toBeInTheDocument();
    expect(screen.getByText("Bob")).toBeInTheDocument();

    await userEvent.click(screen.getByText("Hearts payouts"));
    expect(onSkip).toHaveBeenCalled();
  });

  it("renders nothing when there are no winners", () => {
    const { container } = render(
      <HeartsReveal awards={[]} seats={seats} mySeatIndex={0} variant="mat" onSkip={vi.fn()} />,
    );

    expect(container).toBeEmptyDOMElement();
  });

  it("falls back gracefully when the winning seat is not in the seat list", () => {
    const orphan: HeartsAwardView[] = [
      { pool: "JackOfHearts", seatIndex: 7, chips: 4, cards: [{ suit: "Hearts", rank: "Jack" }] },
    ];
    render(
      <HeartsReveal awards={orphan} seats={seats} mySeatIndex={null} variant="mat" onSkip={vi.fn()} />,
    );

    expect(screen.getAllByText("+4").length).toBeGreaterThan(0);
  });
});
