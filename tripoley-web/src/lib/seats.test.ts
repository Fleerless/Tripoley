import { describe, expect, it } from "vitest";
import type { PlayerSeatView } from "../api/contracts";
import { orderFromSelf, partitionSeats, seatPosition } from "./seats";

function seat(seatIndex: number): PlayerSeatView {
  return { seatIndex, name: `P${seatIndex}`, chipCount: 9, cardsRemaining: 13 };
}

describe("partitionSeats", () => {
  it("excludes self and sorts opponents by seat index", () => {
    const { opponents, self } = partitionSeats([seat(2), seat(0), seat(3), seat(1)], 1);
    expect(opponents.map((s) => s.seatIndex)).toEqual([0, 2, 3]);
    expect(self?.seatIndex).toBe(1);
  });

  it("returns null self and all seats as opponents when mySeatIndex is null", () => {
    const { opponents, self } = partitionSeats([seat(0), seat(1)], null);
    expect(self).toBeNull();
    expect(opponents.map((s) => s.seatIndex)).toEqual([0, 1]);
  });

  it("returns null self when the seat index is not present", () => {
    expect(partitionSeats([seat(0), seat(1)], 5).self).toBeNull();
  });

  it("does not mutate the input array", () => {
    const seats = [seat(2), seat(0), seat(1)];
    partitionSeats(seats, 0);
    expect(seats.map((s) => s.seatIndex)).toEqual([2, 0, 1]);
  });
});

describe("orderFromSelf", () => {
  it("rotates the seats so self comes first", () => {
    const ordered = orderFromSelf([seat(0), seat(1), seat(2), seat(3)], 2);
    expect(ordered.map((s) => s.seatIndex)).toEqual([2, 3, 0, 1]);
  });

  it("returns the seats unchanged when mySeatIndex is null or absent", () => {
    const seats = [seat(0), seat(1)];
    expect(orderFromSelf(seats, null)).toBe(seats);
    expect(orderFromSelf(seats, 9)).toBe(seats);
  });
});

describe("seatPosition", () => {
  it("places the first seat at bottom-center", () => {
    const pos = seatPosition(0, 4);
    expect(pos.left).toBe("50%");
    expect(pos.top).toBe("90%");
  });
});
