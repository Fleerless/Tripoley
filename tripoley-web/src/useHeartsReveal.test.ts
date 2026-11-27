import { act, renderHook } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import type { GameStateView, HeartsAwardView } from "./api/contracts";
import { HEARTS_REVEAL_MS, useHeartsReveal } from "./useHeartsReveal";

const award: HeartsAwardView = {
  pool: "AceOfHearts",
  seatIndex: 0,
  chips: 2,
  cards: [{ suit: "Hearts", rank: "Ace" }],
};

function state(dealNumber: number, heartsAwards: HeartsAwardView[] | null): GameStateView {
  return {
    phase: "MichiganRummyInProgress",
    roundPhase: "AwaitingOpeningLead",
    currentLeaderIndex: 0,
    currentTurnSeatIndex: 0,
    lastPlayedCard: null,
    nextCard: null,
    winnerIndex: null,
    pools: [],
    seats: [],
    dealNumber,
    heartsAwards,
  };
}

beforeEach(() => vi.useFakeTimers());
afterEach(() => vi.useRealTimers());

describe("useHeartsReveal", () => {
  it("surfaces the awards for a new deal and clears them after the timeout", () => {
    const { result, rerender } = renderHook(({ gs }) => useHeartsReveal(gs), {
      initialProps: { gs: null as GameStateView | null },
    });
    expect(result.current.awards).toBeNull();

    rerender({ gs: state(1, [award]) });
    expect(result.current.awards).toEqual([award]);

    act(() => vi.advanceTimersByTime(HEARTS_REVEAL_MS));
    expect(result.current.awards).toBeNull();
  });

  it("does not replay the reveal on another broadcast of the same deal", () => {
    const { result, rerender } = renderHook(({ gs }) => useHeartsReveal(gs), {
      initialProps: { gs: state(1, [award]) as GameStateView | null },
    });
    expect(result.current.awards).toEqual([award]);
    act(() => result.current.skip());
    expect(result.current.awards).toBeNull();

    rerender({ gs: state(1, [award]) });
    expect(result.current.awards).toBeNull();
  });

  it("plays again for the next deal", () => {
    const { result, rerender } = renderHook(({ gs }) => useHeartsReveal(gs), {
      initialProps: { gs: state(1, [award]) as GameStateView | null },
    });
    act(() => result.current.skip());

    rerender({ gs: state(2, [award]) });
    expect(result.current.awards).toEqual([award]);
  });

  it("marks a deal with no winners as shown without opening the reveal", () => {
    const { result, rerender } = renderHook(({ gs }) => useHeartsReveal(gs), {
      initialProps: { gs: null as GameStateView | null },
    });

    rerender({ gs: state(1, []) });
    expect(result.current.awards).toBeNull();
  });
});
