import { describe, expect, it } from "vitest";
import type { CardDto } from "../api/contracts";
import { sortHand } from "./Card";

describe("sortHand", () => {
  it("orders by suit (Clubs, Diamonds, Spades, Hearts) then rank ascending", () => {
    const hand: CardDto[] = [
      { suit: "Hearts", rank: "Two" },
      { suit: "Clubs", rank: "King" },
      { suit: "Clubs", rank: "Three" },
      { suit: "Spades", rank: "Ace" },
      { suit: "Diamonds", rank: "Ten" },
    ];
    const sorted = sortHand(hand).map((c) => `${c.suit}:${c.rank}`);
    expect(sorted).toEqual([
      "Clubs:Three",
      "Clubs:King",
      "Diamonds:Ten",
      "Spades:Ace",
      "Hearts:Two",
    ]);
  });

  it("does not mutate the input", () => {
    const hand: CardDto[] = [
      { suit: "Hearts", rank: "Two" },
      { suit: "Clubs", rank: "King" },
    ];
    sortHand(hand);
    expect(hand[0]).toEqual({ suit: "Hearts", rank: "Two" });
  });
});
