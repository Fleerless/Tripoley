import type { PlayerSeatView } from "../api/contracts";

interface SeatChipProps {
  seat: PlayerSeatView;
  isTurn: boolean;
  isSelf?: boolean;
}

export function SeatChip({ seat, isTurn, isSelf }: SeatChipProps) {
  return (
    <div className={`seat-chip${isTurn ? " active" : ""}${isSelf ? " self" : ""}`}>
      <div className="seat-chip-name">
        {seat.name}
        {isSelf ? " (you)" : ""}
      </div>
      <div className="seat-chip-meta">
        <span className="chip-stack">
          <span className="chip-disc" />
          <span className="chip-disc" />
        </span>
        {seat.chipCount}
        <span className="seat-chip-cards">
          <span className="mini-back" />
          {seat.cardsRemaining}
        </span>
      </div>
    </div>
  );
}
