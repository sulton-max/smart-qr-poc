import { useState } from "react";
import { DEFAULT_PARAMS, HeroCanvas, type HeroMode, type SimParams } from "./HeroCanvas";

const MODES: ReadonlyArray<{ value: HeroMode; label: string }> = [
  { value: "drift", label: "Drift" },
  { value: "bump", label: "Bump" },
  { value: "chase", label: "Mouse chase" },
];

interface Ctrl {
  key: keyof SimParams;
  label: string;
  min: number;
  max: number;
  step: number;
}

const COUNT: Ctrl = { key: "count", label: "count", min: 8, max: 60, step: 1 };
const OPACITY: Ctrl = { key: "opacity", label: "opacity", min: 0.3, max: 1, step: 0.02 };

const PER_MODE: Record<HeroMode, ReadonlyArray<Ctrl>> = {
  drift: [COUNT, { key: "driftSpeed", label: "speed", min: 10, max: 160, step: 2 }, OPACITY],
  bump: [COUNT, { key: "ramSpeed", label: "speed", min: 80, max: 440, step: 5 }, OPACITY],
  chase: [
    COUNT,
    { key: "seekSpeed", label: "speed", min: 80, max: 380, step: 5 },
    { key: "mouseGap", label: "gap radius", min: 6, max: 160, step: 2 },
  ],
};

/**
 * Background layer for the landing hero: the floating-QR simulation + a scrim that keeps the
 * headline legible. Includes a TEMPORARY dev control bar (mode + live sliders that change with the
 * mode) — remove the whole panel once the modes are dialed in and a random-per-refresh picker
 * replaces it.
 */
export function HeroSim() {
  const [mode, setMode] = useState<HeroMode>("drift");
  const [params, setParams] = useState<SimParams>(DEFAULT_PARAMS);
  const controls = PER_MODE[mode];

  return (
    <>
      <HeroCanvas mode={mode} params={params} />

      {/* Scrim — the headline sits on the left, so fade the sim out toward it. */}
      <div
        aria-hidden
        className="pointer-events-none absolute inset-0 z-[1] bg-gradient-to-r from-background via-background/85 to-background/40"
      />
      <div
        aria-hidden
        className="pointer-events-none absolute inset-x-0 bottom-0 z-[1] h-24 bg-gradient-to-t from-background to-transparent"
      />

      {/* TEMP — dev control bar; remove once modes are dialed in. */}
      <div className="absolute left-4 top-4 z-20 flex max-w-[min(92vw,40rem)] flex-wrap items-center gap-x-3 gap-y-1.5 rounded-lg border border-border bg-card/90 px-3 py-2 text-xs text-muted-foreground shadow-sm backdrop-blur">
        <label className="inline-flex items-center gap-1.5">
          <span className="opacity-60">sim</span>
          <select
            value={mode}
            onChange={(e) => setMode(e.target.value as HeroMode)}
            className="bg-transparent font-medium text-foreground outline-none"
            aria-label="Hero simulation mode (dev)"
          >
            {MODES.map((m) => (
              <option key={m.value} value={m.value}>
                {m.label}
              </option>
            ))}
          </select>
        </label>

        {controls.map((c) => (
          <label key={c.key} className="inline-flex items-center gap-1.5">
            <span className="opacity-60">{c.label}</span>
            <input
              type="range"
              min={c.min}
              max={c.max}
              step={c.step}
              value={params[c.key]}
              onChange={(e) => setParams((p) => ({ ...p, [c.key]: Number(e.target.value) }))}
              className="h-1 w-16 accent-primary"
              aria-label={c.label}
            />
            <span className="w-8 tabular-nums text-foreground/70">
              {c.step < 1 ? params[c.key].toFixed(2) : params[c.key]}
            </span>
          </label>
        ))}
      </div>
    </>
  );
}
