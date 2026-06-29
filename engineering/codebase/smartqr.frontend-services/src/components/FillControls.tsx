import { SegmentedControl, ToggleButton } from "@wow-two-beta/ui/actions";
import { Stack } from "@wow-two-beta/ui/layout";
import { cn } from "@wow-two-beta/ui/utils";
import { ArrowDown, ArrowDownLeft, ArrowDownRight, ArrowLeftRight, ArrowRight } from "lucide-react";
import type { ComponentType } from "react";
import { GradientType, type PreviewGradient } from "../types";
import { TileColorPicker } from "./TileColorPicker";

export interface FillControlsProps {
  /** The solid foreground — seeds the gradient's first stop when first enabled. */
  foreground: string;
  /** Emit the next solid foreground. */
  onForegroundChange: (hex: string) => void;
  /** The current foreground gradient, or `null` for a solid foreground. */
  gradient: PreviewGradient | null;
  /** Emit the next gradient, or `null` to fall back to the solid foreground. */
  onGradientChange: (gradient: PreviewGradient | null) => void;
}

const FILL_SOLID = "solid";
const FILL_GRADIENT = "gradient";

// Linear-gradient direction presets — angle (deg) paired with a directional arrow.
const ANGLES: { value: number; Icon: ComponentType<{ size?: number }>; label: string }[] = [
  { value: 0, Icon: ArrowRight, label: "Left to right" },
  { value: 45, Icon: ArrowDownRight, label: "Diagonal down-right" },
  { value: 90, Icon: ArrowDown, label: "Top to bottom" },
  { value: 135, Icon: ArrowDownLeft, label: "Diagonal down-left" },
];

// Radial-gradient radius presets (0..1 extent). Frontend-only until the backend wires it.
const RADII: { value: number; label: string }[] = [
  { value: 0.4, label: "Tight" },
  { value: 0.6, label: "Compact" },
  { value: 0.8, label: "Wide" },
  { value: 1.0, label: "Full" },
];

const DEFAULT_RADIUS = 0.8;

/** A muted left label paired with a right-aligned control; rows stack with a hairline divider. */
function Row({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div className="flex min-h-10 items-center justify-between gap-4 py-2 first:pt-0 [&:not(:last-child)]:border-b [&:not(:last-child)]:border-border">
      <span className="text-sm text-muted-foreground">{label}</span>
      <div className="flex items-center gap-2">{children}</div>
    </div>
  );
}

/** A real preset button with the SDK's soft-primary active treatment when selected. */
function PresetButton({
  selected,
  ariaLabel,
  title,
  disabled,
  onClick,
  children,
}: {
  selected: boolean;
  ariaLabel: string;
  title: string;
  disabled?: boolean;
  onClick: () => void;
  children: React.ReactNode;
}) {
  return (
    <button
      type="button"
      aria-pressed={selected}
      aria-label={ariaLabel}
      title={title}
      disabled={disabled}
      onClick={onClick}
      className={cn(
        "inline-flex h-8 w-8 items-center justify-center rounded-md border text-sm transition-colors",
        "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring",
        "disabled:cursor-not-allowed disabled:opacity-60",
        selected
          ? "border-primary bg-primary/10 text-primary"
          : "border-border bg-background text-muted-foreground hover:border-border-strong hover:text-foreground",
      )}
    >
      {children}
    </button>
  );
}

/**
 * A borderless arrow between the two gradient swatches that reverses the stops on click.
 * Idle: muted, transparent, a single right arrow. On hover/focus-visible: a bordered chip
 * with a left↔right swap glyph. Keyboard-operable (real button + focus-visible affordance).
 */
function SwapConnector({ onSwap }: { onSwap: () => void }) {
  return (
    <button
      type="button"
      aria-label="Reverse gradient colors"
      onClick={onSwap}
      className={cn(
        "group inline-flex h-7 w-7 items-center justify-center rounded-md border border-transparent bg-transparent text-muted-foreground transition-colors",
        "hover:border-border hover:bg-background hover:text-foreground",
        "focus-visible:border-border focus-visible:bg-background focus-visible:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring",
      )}
    >
      <ArrowRight size={16} className="group-hover:hidden group-focus-visible:hidden" />
      <ArrowLeftRight size={16} className="hidden group-hover:inline group-focus-visible:inline" />
    </button>
  );
}

/**
 * Foreground-fill controls — switch between a solid color and a two-stop linear/radial
 * gradient, in a dense label-to-value rows layout. Drives the live preview `style.gradient`
 * (`null` = solid). Background sits in its own row in the parent.
 */
export function FillControls({ foreground, onForegroundChange, gradient, onGradientChange }: FillControlsProps) {
  const isGradient = gradient !== null;

  function enable() {
    onGradientChange({
      type: GradientType.Linear,
      angle: 45,
      radius: DEFAULT_RADIUS,
      stops: [
        { color: foreground, offset: 0 },
        { color: "#7c3aed", offset: 1 },
      ],
    });
  }

  function patch(next: Partial<PreviewGradient>) {
    if (gradient) onGradientChange({ ...gradient, ...next });
  }

  function setStop(index: 0 | 1, color: string) {
    if (!gradient) return;
    const stops = gradient.stops.slice();
    stops[index] = { ...stops[index], color };
    onGradientChange({ ...gradient, stops });
  }

  function swapStops() {
    if (!gradient) return;
    const [a, b] = gradient.stops;
    if (!a || !b) return;
    // Swap the colors but keep each stop's offset, so from↔to flips visually.
    onGradientChange({
      ...gradient,
      stops: [
        { ...a, color: b.color },
        { ...b, color: a.color },
      ],
    });
  }

  const isLinear = gradient?.type === GradientType.Linear;
  const isRadial = gradient?.type === GradientType.Radial;
  const fromColor = gradient?.stops[0]?.color ?? foreground;
  const toColor = gradient?.stops[1]?.color ?? "#7c3aed";

  return (
    <Stack gap="0">
      <Row label="Fill">
        <SegmentedControl
          type="single"
          value={isGradient ? FILL_GRADIENT : FILL_SOLID}
          onValueChange={(v) => {
            if (v === FILL_GRADIENT) enable();
            else if (v === FILL_SOLID) onGradientChange(null);
          }}
          aria-label="Foreground fill"
        >
          <ToggleButton value={FILL_SOLID} size="sm">Solid</ToggleButton>
          <ToggleButton value={FILL_GRADIENT} size="sm">Gradient</ToggleButton>
        </SegmentedControl>
      </Row>

      <Row label="Colors">
        {isGradient ? (
          <>
            <TileColorPicker value={fromColor} onValueChange={(hex) => setStop(0, hex)} ariaLabel="Gradient start color" />
            <SwapConnector onSwap={swapStops} />
            <TileColorPicker value={toColor} onValueChange={(hex) => setStop(1, hex)} ariaLabel="Gradient end color" />
          </>
        ) : (
          <TileColorPicker value={foreground} onValueChange={onForegroundChange} ariaLabel="Foreground color" />
        )}
      </Row>

      {isGradient && (
        // Geometry: a real connected vertical Linear/Radial SegmentedControl on the left spanning
        // both preset rows; each row holds one type's presets with a hairline between. The unselected
        // row is disabled in place. The toggle itself is the only label (no Direction/Radius text).
        <div className="flex items-stretch gap-3 py-2">
          <SegmentedControl
            type="single"
            orientation="vertical"
            value={gradient.type}
            onValueChange={(v) => v && patch({ type: v as PreviewGradient["type"] })}
            aria-label="Gradient type"
            className="shrink-0 self-stretch"
          >
            <ToggleButton value={GradientType.Linear} size="sm" className="flex-1">
              Linear
            </ToggleButton>
            <ToggleButton value={GradientType.Radial} size="sm" className="flex-1">
              Radial
            </ToggleButton>
          </SegmentedControl>

          <div className="flex flex-1 flex-col">
            <div
              aria-disabled={!isLinear}
              className={cn(
                "flex items-center justify-end gap-2 pb-2",
                !isLinear && "pointer-events-none opacity-40",
              )}
            >
              {ANGLES.map((a) => (
                <PresetButton
                  key={a.value}
                  selected={!!isLinear && gradient.angle === a.value}
                  ariaLabel={a.label}
                  title={`${a.value}°`}
                  disabled={!isLinear}
                  onClick={() => patch({ angle: a.value })}
                >
                  <a.Icon size={16} />
                </PresetButton>
              ))}
            </div>
            <div className="border-t border-border" />
            <div
              aria-disabled={!isRadial}
              className={cn(
                "flex items-center justify-end gap-2 pt-2",
                !isRadial && "pointer-events-none opacity-40",
              )}
            >
              {RADII.map((r) => (
                <PresetButton
                  key={r.value}
                  selected={!!isRadial && (gradient.radius ?? DEFAULT_RADIUS) === r.value}
                  ariaLabel={`${r.label} radius`}
                  title={r.label}
                  disabled={!isRadial}
                  onClick={() => patch({ radius: r.value })}
                >
                  <RadiusGlyph extent={r.value} />
                </PresetButton>
              ))}
            </div>
          </div>
        </div>
      )}
    </Stack>
  );
}

/** A concentric-circle glyph whose inner radius scales with the preset's extent (0..1). */
function RadiusGlyph({ extent }: { extent: number }) {
  return (
    <svg viewBox="0 0 24 24" width="16" height="16" aria-hidden="true">
      <circle cx="12" cy="12" r="10" fill="none" stroke="currentColor" strokeWidth="1.5" opacity="0.4" />
      <circle cx="12" cy="12" r={Math.max(2, 10 * extent)} fill="currentColor" />
    </svg>
  );
}
