import type { CardDto, PlayerSeatView } from "../api/contracts";
import { Card, cardKey, sortHand } from "./Card";

interface SeatProps {
  seat: PlayerSeatView;
  isSelf: boolean;
  isTurn: boolean;
  position: { left: string; top: string };
  hand?: CardDto[];
  legalPlays?: CardDto[];
  onPlay?: (card: CardDto) => void;
}

const sameCard = (a: CardDto, b: CardDto) => a.suit === b.suit && a.rank === b.rank;

export function Seat({ seat, isSelf, isTurn, position, hand, legalPlays = [], onPlay }: SeatProps) {
  return (
    <div className={`seat${isTurn ? " active" : ""}`} style={{ left: position.left, top: position.top }}>
      <div className="nameplate">
        <div className="seat-name">
          {seat.name}
          {isSelf ? " (you)" : ""}
        </div>
        <div className="seat-chips">
          <span className="chip-stack">
            <span className="chip-disc" />
            <span className="chip-disc" />
            <span className="chip-disc" />
          </span>
          {seat.chipCount}
          <span className="seat-meta">· {seat.cardsRemaining} cards</span>
        </div>
      </div>

      <div className={isSelf ? "seat-cards" : "seat-cards backs"}>
        {isSelf && hand
          ? sortHand(hand).map((card) => (
              <Card
                key={cardKey(card)}
                card={card}
                highlighted={legalPlays.some((p) => sameCard(p, card))}
                onClick={onPlay ? () => onPlay(card) : undefined}
              />
            ))
          : Array.from({ length: seat.cardsRemaining }).map((_, i) => <Card key={i} back />)}
      </div>
    </div>
  );
}
