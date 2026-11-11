export const CARD_WIDTH = 50;
export const MAX_STEP = 42;

export function computeHandStep(trackWidth: number, cardCount: number): number {
  if (cardCount <= 1 || trackWidth <= 0) return MAX_STEP;
  return Math.min(MAX_STEP, (trackWidth - CARD_WIDTH - 1) / (cardCount - 1));
}
