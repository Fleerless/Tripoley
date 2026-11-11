import { LayoutGroup } from "motion/react";
import type { Game } from "../useGame";
import { orderFromSelf, partitionSeats, seatPosition } from "../lib/seats";
import { useIsMobile } from "../useIsMobile";
import { CenterPile } from "./CenterPile";
import { HandDock } from "./HandDock";
import { Pools } from "./Pools";
import { Seat } from "./Seat";
import { SeatChip } from "./SeatChip";

export function Table({ game }: { game: Game }) {
  const gs = game.gameState;
  const mySeatIndex = game.hand?.seatIndex ?? null;
  const isMobile = useIsMobile();

  if (isMobile) {
    const { opponents, self } = partitionSeats(gs?.seats ?? [], mySeatIndex);
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
