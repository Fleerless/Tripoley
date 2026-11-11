import { useLayoutEffect, useRef, useState, type CSSProperties } from "react";
import type { CardDto } from "../api/contracts";
import { CARD_WIDTH, computeHandStep } from "../lib/handLayout";
import { Card, cardKey, sortHand } from "./Card";

interface HandDockProps {
  hand: CardDto[];
  legalPlays: CardDto[];
  onPlay?: (card: CardDto) => void;
}

const sameCard = (a: CardDto, b: CardDto) => a.suit === b.suit && a.rank === b.rank;

export function HandDock({ hand, legalPlays, onPlay }: HandDockProps) {
  const trackRef = useRef<HTMLDivElement>(null);
  const [trackWidth, setTrackWidth] = useState(0);

  useLayoutEffect(() => {
    const el = trackRef.current;
    if (!el) return;
    const observer = new ResizeObserver(([entry]) => setTrackWidth(entry.contentRect.width));
    observer.observe(el);
    setTrackWidth(el.getBoundingClientRect().width);
    return () => observer.disconnect();
  }, []);

  const cards = sortHand(hand);
  const overlap = computeHandStep(trackWidth, cards.length) - CARD_WIDTH;

  return (
    <div className="hand-dock">
      <div
        ref={trackRef}
        className="hand-dock-track"
        style={{ "--dock-overlap": `${overlap}px` } as CSSProperties}
      >
        {cards.map((card) => (
          <Card
            key={cardKey(card)}
            card={card}
            highlighted={legalPlays.some((p) => sameCard(p, card))}
            onClick={onPlay ? () => onPlay(card) : undefined}
          />
        ))}
      </div>
    </div>
  );
}
