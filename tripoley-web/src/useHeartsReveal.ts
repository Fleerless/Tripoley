import { useEffect, useRef, useState } from "react";
import type { GameStateView, HeartsAwardView } from "./api/contracts";

export const HEARTS_REVEAL_MS = 3200;

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
