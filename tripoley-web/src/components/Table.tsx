import { LayoutGroup } from "motion/react";
import type { PlayerSeatView } from "../api/contracts";
import type { Game } from "../useGame";
import { useIsMobile } from "../useIsMobile";
import { CenterPile } from "./CenterPile";
import { HandDock } from "./HandDock";
import { Pools } from "./Pools";
import { Seat } from "./Seat";
import { SeatChip } from "./SeatChip";

function orderFromSelf(seats: PlayerSeatView[], mySeatIndex: number | null): PlayerSeatView[] {
  if (mySeatIndex === null) return seats;
  const start = seats.findIndex((s) => s.seatIndex === mySeatIndex);
  if (start < 0) return seats;
  return [...seats.slice(start), ...seats.slice(0, start)];
}

function seatPosition(displayIndex: number, total: number): { left: string; top: string } {
  const angle = Math.PI / 2 + (2 * Math.PI * displayIndex) / total;
  return {
    left: `${50 + 42 * Math.cos(angle)}%`,
    top: `${50 + 40 * Math.sin(angle)}%`,
  };
}

export function Table({ game }: { game: Game }) {
  const gs = game.gameState;
  const mySeatIndex = game.hand?.seatIndex ?? null;
  const isMobile = useIsMobile();

  if (isMobile) {
    const seats = gs?.seats ?? [];
    const opponents = seats
      .filter((s) => s.seatIndex !== mySeatIndex)
      .sort((a, b) => a.seatIndex - b.seatIndex);
    const self = mySeatIndex === null ? null : seats.find((s) => s.seatIndex === mySeatIndex);
    return (
      <LayoutGroup>
        <div className="table-mobile">
          <div className="opp-strip">
            {opponents.map((seat) => (
              <SeatChip
                key={seat.seatIndex}
                seat={seat}
                isTurn={seat.seatIndex === gs?.currentTurnSeatIndex}
              />
            ))}
          </div>

          <div className="mobile-center">
            <Pools pools={gs?.pools ?? []} className="board-mobile" />
            <CenterPile card={gs?.lastPlayedCard ?? null} />
          </div>

          {self && (
            <div className="self-row">
              <SeatChip seat={self} isTurn={self.seatIndex === gs?.currentTurnSeatIndex} isSelf />
            </div>
          )}

          <HandDock
            hand={game.hand?.hand ?? []}
            legalPlays={game.hand?.legalPlays ?? []}
            onPlay={game.playCard}
          />
        </div>
      </LayoutGroup>
    );
  }

  const ordered = orderFromSelf(gs?.seats ?? [], mySeatIndex);

  return (
    <LayoutGroup>
      <div className="mat">
        <Pools pools={gs?.pools ?? []} />
        <CenterPile card={gs?.lastPlayedCard ?? null} />

        {ordered.map((seat, i) => {
          const isSelf = seat.seatIndex === mySeatIndex;
          return (
            <Seat
              key={seat.seatIndex}
              seat={seat}
              isSelf={isSelf}
              isTurn={seat.seatIndex === gs?.currentTurnSeatIndex}
              position={seatPosition(i, ordered.length)}
              hand={isSelf ? game.hand?.hand : undefined}
              legalPlays={isSelf ? game.hand?.legalPlays : undefined}
              onPlay={isSelf ? game.playCard : undefined}
            />
          );
        })}
      </div>
    </LayoutGroup>
  );
}
