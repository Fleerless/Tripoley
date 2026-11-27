import { useEffect, useRef, useState } from "react";
import type { GameStateView, HeartsAwardView } from "./api/contracts";

// How long the pay-card reveal stays on screen before it fades on its own.
export const HEARTS_REVEAL_MS = 3200;

// The Hearts settlement rides on every GameStateView for the whole deal, so we play the reveal
// exactly once per deal (keyed by dealNumber) rather than on every broadcast. A fresh client
// (reconnect / refresh) starts from deal 0 and so replays the current deal's reveal once, which
// is fine: the durable pool/chip totals in the state are already correct either way.
export function useHeartsReveal(gameState: GameStateView | null): {
  awards: HeartsAwardView[] | null;
  skip: () => void;
} {
  const [awards, setAwards] = useState<HeartsAwardView[] | null>(null);
  const shownDealRef = useRef(0);
  const timerRef = useRef<ReturnType<typeof setTimeout>>(undefined);

  useEffect(() => {
    if (!gameState) return;
    const { dealNumber, heartsAwards } = gameState;
    if (dealNumber <= shownDealRef.current) return;
    shownDealRef.current = dealNumber;

    if (!heartsAwards || heartsAwards.length === 0) return;
    setAwards(heartsAwards);
    clearTimeout(timerRef.current);
    timerRef.current = setTimeout(() => setAwards(null), HEARTS_REVEAL_MS);
  }, [gameState]);

  useEffect(() => () => clearTimeout(timerRef.current), []);

  const skip = () => {
    clearTimeout(timerRef.current);
    setAwards(null);
  };

  return { awards, skip };
}
