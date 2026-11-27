import { motion } from "motion/react";
import type { CardDto, HeartsAwardView, PlayerSeatView } from "../api/contracts";
import { orderFromSelf, seatPosition } from "../lib/seats";
import { Card } from "./Card";

interface SeatWinnings {
  seatIndex: number;
  name: string;
  chips: number;
  cards: CardDto[];
}

const sameCard = (a: CardDto, b: CardDto) => a.suit === b.suit && a.rank === b.rank;

function winningsBySeat(awards: HeartsAwardView[], seats: PlayerSeatView[]): SeatWinnings[] {
  const bySeat = new Map<number, SeatWinnings>();
  for (const award of awards) {
    const name = seats.find((s) => s.seatIndex === award.seatIndex)?.name ?? "";
    const entry = bySeat.get(award.seatIndex) ?? { seatIndex: award.seatIndex, name, chips: 0, cards: [] };
    entry.chips += award.chips;
    for (const card of award.cards) {
      if (!entry.cards.some((existing) => sameCard(existing, card))) entry.cards.push(card);
    }
    bySeat.set(award.seatIndex, entry);
  }
  return [...bySeat.values()];
}

interface HeartsRevealProps {
  awards: HeartsAwardView[];
  seats: PlayerSeatView[];
  mySeatIndex: number | null;
  variant: "mat" | "mobile";
  onSkip: () => void;
}

export function HeartsReveal({ awards, seats, mySeatIndex, variant, onSkip }: HeartsRevealProps) {
  const winnings = winningsBySeat(awards, seats);
  if (winnings.length === 0) return null;

  if (variant === "mobile") {
    return (
      <div className="hearts-reveal hearts-reveal-mobile" onClick={onSkip}>
        <div className="hearts-reveal-title">Hearts payouts</div>
        {winnings.map((w) => (
          <div key={w.seatIndex} className="hearts-reveal-row">
            <span className="hearts-reveal-name">{w.name}</span>
            <span className="hearts-reveal-row-cards">
              {w.cards.map((c) => (
                <Card key={`${c.suit}-${c.rank}`} card={c} />
              ))}
            </span>
            <span className="hearts-reveal-badge">+{w.chips}</span>
          </div>
        ))}
        <div className="hearts-reveal-hint">tap to skip</div>
      </div>
    );
  }

  const ordered = orderFromSelf(seats, mySeatIndex);
  const positionOf = (seatIndex: number) => {
    const displayIndex = ordered.findIndex((s) => s.seatIndex === seatIndex);
    return seatPosition(displayIndex < 0 ? 0 : displayIndex, ordered.length);
  };

  return (
    <div className="hearts-reveal hearts-reveal-mat">
      {winnings.map((w) => {
        const { left, top } = positionOf(w.seatIndex);
        return (
          <div key={w.seatIndex}>
            <motion.div
              className="hearts-reveal-chip"
              initial={{ left: "50%", top: "50%", opacity: 0, scale: 0.4 }}
              animate={{ left, top, opacity: [0, 1, 1, 0], scale: 1 }}
              transition={{ duration: 1.1, times: [0, 0.2, 0.8, 1] }}
            >
              +{w.chips}
            </motion.div>
            <motion.div
              className="hearts-reveal-seat"
              style={{ left, top }}
              initial={{ opacity: 0, y: 8 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0 }}
              transition={{ delay: 0.5 }}
            >
              <div className="hearts-reveal-cards">
                {w.cards.map((c) => (
                  <Card key={`${c.suit}-${c.rank}`} card={c} />
                ))}
              </div>
              <div className="hearts-reveal-badge">+{w.chips}</div>
            </motion.div>
          </div>
        );
      })}
      <button type="button" className="hearts-reveal-hint" onClick={onSkip}>
        click to skip
      </button>
    </div>
  );
}
