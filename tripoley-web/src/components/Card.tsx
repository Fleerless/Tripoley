import { motion } from "motion/react";
import type { CardDto, Rank, Suit } from "../api/contracts";

const SUIT_SYMBOL: Record<Suit, string> = {
  Clubs: "♣",
  Diamonds: "♦",
  Hearts: "♥",
  Spades: "♠",
};

const RANK_SHORT: Record<Rank, string> = {
  Two: "2", Three: "3", Four: "4", Five: "5", Six: "6", Seven: "7",
  Eight: "8", Nine: "9", Ten: "10", Jack: "J", Queen: "Q", King: "K", Ace: "A",
};

export function cardKey(card: CardDto): string {
  return `card-${card.suit}-${card.rank}`;
}

const SUIT_ORDER: Record<Suit, number> = { Clubs: 0, Diamonds: 1, Spades: 2, Hearts: 3 };
const RANK_ORDER: Record<Rank, number> = {
  Two: 2, Three: 3, Four: 4, Five: 5, Six: 6, Seven: 7, Eight: 8,
  Nine: 9, Ten: 10, Jack: 11, Queen: 12, King: 13, Ace: 14,
};

export function sortHand(cards: CardDto[]): CardDto[] {
  return [...cards].sort(
    (a, b) => SUIT_ORDER[a.suit] - SUIT_ORDER[b.suit] || RANK_ORDER[a.rank] - RANK_ORDER[b.rank],
  );
}

interface CardProps {
  card?: CardDto;
  back?: boolean;
  highlighted?: boolean;
  onClick?: () => void;
}

export function Card({ card, back, highlighted, onClick }: CardProps) {
  const isRed = card ? card.suit === "Hearts" || card.suit === "Diamonds" : false;
  const className = [
    "card",
    back ? "card-back" : "card-face",
    isRed ? "red" : "",
    highlighted ? "card-playable" : "",
  ]
    .filter(Boolean)
    .join(" ");

  return (
    <motion.div
      layout
      layoutId={card && !back ? cardKey(card) : undefined}
      onClick={onClick}
      className={className}
    >
      {!back && card && (
        <>
          <span className="card-index">
            <span className="card-rank">{RANK_SHORT[card.rank]}</span>
            <span className="card-suit">{SUIT_SYMBOL[card.suit]}</span>
          </span>
          <span className="card-pip">{SUIT_SYMBOL[card.suit]}</span>
        </>
      )}
    </motion.div>
  );
}
