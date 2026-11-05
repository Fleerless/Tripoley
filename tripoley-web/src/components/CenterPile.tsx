import { AnimatePresence } from "motion/react";
import type { CardDto } from "../api/contracts";
import { Card, cardKey } from "./Card";

export function CenterPile({ card }: { card: CardDto | null }) {
  return (
    <div className="center-pile">
      <AnimatePresence>{card && <Card key={cardKey(card)} card={card} />}</AnimatePresence>
    </div>
  );
}
