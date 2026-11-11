import { describe, expect, it } from "vitest";
import { CARD_WIDTH, MAX_STEP, computeHandStep } from "./handLayout";

describe("computeHandStep", () => {
  it("returns MAX_STEP for an empty or single-card hand", () => {
    expect(computeHandStep(351, 0)).toBe(MAX_STEP);
    expect(computeHandStep(351, 1)).toBe(MAX_STEP);
  });

  it("returns MAX_STEP before the track has been measured", () => {
    expect(computeHandStep(0, 13)).toBe(MAX_STEP);
  });

  it("caps at MAX_STEP when few cards leave room to spare", () => {
    expect(computeHandStep(351, 3)).toBe(MAX_STEP);
  });

  it("compresses below MAX_STEP when many cards must fit", () => {
    const step = computeHandStep(351, 17);
    expect(step).toBeLessThan(MAX_STEP);
    expect(step).toBeGreaterThan(0);
  });

  it("never lets the hand exceed the track width", () => {
    const track = 351;
    for (const n of [2, 5, 9, 13, 17, 26]) {
      const total = CARD_WIDTH + (n - 1) * computeHandStep(track, n);
      expect(total).toBeLessThanOrEqual(track);
    }
  });
});
