import type { PlayerSeatView } from "../api/contracts";

export function partitionSeats(
  seats: PlayerSeatView[],
  mySeatIndex: number | null,
): { opponents: PlayerSeatView[]; self: PlayerSeatView | null } {
  const opponents = seats
    .filter((s) => s.seatIndex !== mySeatIndex)
    .sort((a, b) => a.seatIndex - b.seatIndex);
  const self =
    mySeatIndex === null ? null : (seats.find((s) => s.seatIndex === mySeatIndex) ?? null);
  return { opponents, self };
}

export function orderFromSelf(
  seats: PlayerSeatView[],
  mySeatIndex: number | null,
): PlayerSeatView[] {
  if (mySeatIndex === null) return seats;
  const start = seats.findIndex((s) => s.seatIndex === mySeatIndex);
  if (start < 0) return seats;
  return [...seats.slice(start), ...seats.slice(0, start)];
}

export function seatPosition(displayIndex: number, total: number): { left: string; top: string } {
  const angle = Math.PI / 2 + (2 * Math.PI * displayIndex) / total;
  return {
    left: `${50 + 42 * Math.cos(angle)}%`,
    top: `${50 + 40 * Math.sin(angle)}%`,
  };
}
