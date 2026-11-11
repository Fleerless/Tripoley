import type { ChipPoolType, PoolView } from "../api/contracts";

const C = 170;
const R_OUTER = 164;
const R_INNER = 62;
const R_LABEL = (R_OUTER + R_INNER) / 2;

const FELT = "#103a2e";
const FELT_DK = "#0b2e24";
const BRASS = "#d9b36b";
const CREAM = "#efe6cf";
const AMBER = "#ffb15c";
const RED = "#d6475a";

interface Wedge {
  type: ChipPoolType;
  label: string;
  hearts: number;
}

const WEDGES: Wedge[] = [
  { type: "KingQueenOfHearts", label: "KING-QUEEN", hearts: 2 },
  { type: "Kitty", label: "KITTY", hearts: 0 },
  { type: "TenOfHearts", label: "TEN", hearts: 1 },
  { type: "JackOfHearts", label: "JACK", hearts: 1 },
  { type: "QueenOfHearts", label: "QUEEN", hearts: 1 },
  { type: "KingOfHearts", label: "KING", hearts: 1 },
  { type: "AceOfHearts", label: "ACE", hearts: 1 },
  { type: "EightNineTen", label: "8·9·10", hearts: 0 },
];

function pt(angleDeg: number, radius: number) {
  const a = (angleDeg * Math.PI) / 180;
  return { x: C + radius * Math.cos(a), y: C + radius * Math.sin(a) };
}

function labelRotation(angleDeg: number): number {
  let rot = angleDeg - 90;
  if (rot < -90) rot += 180;
  if (rot > 90) rot -= 180;
  return rot;
}

export function Pools({ pools, className = "board" }: { pools: PoolView[]; className?: string }) {
  const count = (t: ChipPoolType) => pools.find((p) => p.type === t)?.chipCount ?? 0;

  return (
    <svg className={className} viewBox="0 0 340 340">
      <circle cx={C} cy={C} r={R_OUTER} fill={FELT} stroke={BRASS} strokeWidth={5} />
      <circle cx={C} cy={C} r={R_OUTER - 6} fill="none" stroke={CREAM} strokeWidth={1} opacity={0.6} />

      {WEDGES.map((_, i) => {
        const boundary = -90 - 22.5 + i * 45;
        const a = pt(boundary, R_INNER);
        const b = pt(boundary, R_OUTER);
        return <line key={i} x1={a.x} y1={a.y} x2={b.x} y2={b.y} stroke={BRASS} strokeWidth={2} />;
      })}

      {WEDGES.map((w, i) => {
        const angle = -90 + i * 45;
        const p = pt(angle, R_LABEL);
        const labelY = w.hearts > 0 ? 0 : -9;
        const countY = w.hearts > 0 ? 17 : 9;
        const isLongLabel = w.label.length > 7;
        return (
          <g key={w.type} transform={`translate(${p.x} ${p.y}) rotate(${labelRotation(angle)})`}>
            {w.hearts > 0 && (
              <text textAnchor="middle" dominantBaseline="central" y={-17} fontSize={14} fill={RED}>
                {"♥".repeat(w.hearts)}
              </text>
            )}
            <text
              textAnchor="middle"
              dominantBaseline="central"
              y={labelY}
              fontSize={11}
              fill={CREAM}
              letterSpacing="1"
              textLength={isLongLabel ? 78 : undefined}
              lengthAdjust="spacingAndGlyphs"
            >
              {w.label}
            </text>
            <text
              textAnchor="middle"
              dominantBaseline="central"
              y={countY}
              fontSize={13}
              fontWeight="bold"
              fill={AMBER}
            >
              {count(w.type)}
            </text>
          </g>
        );
      })}

      <circle cx={C} cy={C} r={R_INNER} fill={FELT_DK} stroke={BRASS} strokeWidth={4} />
      <circle cx={C} cy={C} r={R_INNER - 5} fill="none" stroke={CREAM} strokeWidth={1} opacity={0.5} />
      <text x={C} y={C - 6} textAnchor="middle" fontSize={20} fontWeight="bold" fill={BRASS} letterSpacing="2">
        POT
      </text>
      <text x={C} y={C + 16} textAnchor="middle" fontSize={15} fontWeight="bold" fill={AMBER}>
        {count("Pot")}
      </text>
    </svg>
  );
}
