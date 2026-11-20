import type { PlayerSeatView } from "../api/contracts";

interface SeatChipProps {
  seat: PlayerSeatView;
  isTurn: boolean;
  isSelf?: boolean;
}

export function SeatChip({ seat, isTurn, isSelf }: SeatChipProps) {
  const active = seat.presence === "Active";
  return (
    <div className={`seat-chip${isTurn ? " active" : ""}${isSelf ? " self" : ""}${active ? "" : " away"}`}>
      <div className="seat-chip-name">
        <span className={`conn-dot${active ? " online" : ""}`} />
        {seat.name}
        {isSelf ? " (you)" : ""}
        {seat.presence === "Abandoned" && <span className="seat-away-tag">left</span>}
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
