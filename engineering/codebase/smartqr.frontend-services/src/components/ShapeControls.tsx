import type { ReactNode } from "react";
import { cn } from "@wow-two-beta/ui/utils";
import { Stack } from "@wow-two-beta/ui/layout";
import { FinderShape, ModuleShape } from "../types";

export interface ShapeControlsProps {
  moduleShape: ModuleShape;
  finderShape: FinderShape;
  finderDotShape: FinderShape;
  onModuleShapeChange: (shape: ModuleShape) => void;
  onFinderShapeChange: (shape: FinderShape) => void;
  onFinderDotShapeChange: (shape: FinderShape) => void;
}

const MODULE_LABEL: Record<ModuleShape, string> = {
  square: "Square",
  rounded: "Rounded",
  dots: "Dots",
  classy: "Classy",
  classyRounded: "Classy rounded",
  verticalBars: "Vertical bars",
  horizontalBars: "Horizontal bars",
};

const MODULE_ORDER: ModuleShape[] = [
  ModuleShape.Square,
  ModuleShape.Rounded,
  ModuleShape.Dots,
  ModuleShape.Classy,
  ModuleShape.ClassyRounded,
  ModuleShape.VerticalBars,
  ModuleShape.HorizontalBars,
];

const FINDER_LABEL: Record<FinderShape, string> = {
  square: "Square",
  rounded: "Rounded",
  circle: "Circle",
};

const FINDER_ORDER: FinderShape[] = [FinderShape.Square, FinderShape.Rounded, FinderShape.Circle];

/**
 * A tiny 24×24 swatch that previews a module shape as a 3-cell row of `currentColor`
 * cells — a representative slice of how that shape tiles the symbol.
 */
function ModuleSwatch({ shape }: { shape: ModuleShape }) {
  // Three cells at x = 3 / 9.5 / 16, each 5 wide on a 24-unit canvas.
  const xs = [3, 9.5, 16];
  const w = 5;

  if (shape === ModuleShape.Dots) {
    return (
      <svg viewBox="0 0 24 24" width="20" height="20" aria-hidden="true">
        {xs.map((x) => (
          <circle key={x} cx={x + w / 2} cy={12} r={2.5} fill="currentColor" />
        ))}
      </svg>
    );
  }

  if (shape === ModuleShape.VerticalBars) {
    return (
      <svg viewBox="0 0 24 24" width="20" height="20" aria-hidden="true">
        {xs.map((x) => (
          <rect key={x} x={x} y={4} width={w} height={16} rx={2.4} fill="currentColor" />
        ))}
      </svg>
    );
  }

  if (shape === ModuleShape.HorizontalBars) {
    const ys = [3, 9.5, 16];
    return (
      <svg viewBox="0 0 24 24" width="20" height="20" aria-hidden="true">
        {ys.map((y) => (
          <rect key={y} x={4} y={y} width={16} height={w} rx={2.4} fill="currentColor" />
        ))}
      </svg>
    );
  }

  // square / rounded / classy / classyRounded → three cells differing only by corner radius.
  const rx =
    shape === ModuleShape.Rounded
      ? 1.6
      : shape === ModuleShape.Classy
        ? 0.8
        : shape === ModuleShape.ClassyRounded
          ? 2.2
          : 0; // square
  return (
    <svg viewBox="0 0 24 24" width="20" height="20" aria-hidden="true">
      {xs.map((x) => (
        <rect key={x} x={x} y={9.5} width={w} height={w} rx={rx} fill="currentColor" />
      ))}
    </svg>
  );
}

/** A finder-eye swatch: an outer ring frame + an inner pupil, both in `currentColor`. */
function FinderSwatch({ shape, dot }: { shape: FinderShape; dot?: boolean }) {
  // dot = render only the inner pupil emphasis; otherwise the full eye (frame + pupil).
  const frameRx = shape === FinderShape.Square ? 0 : shape === FinderShape.Rounded ? 4 : 11;
  const dotRx = shape === FinderShape.Square ? 0 : shape === FinderShape.Rounded ? 1.6 : 4;
  return (
    <svg viewBox="0 0 24 24" width="20" height="20" aria-hidden="true">
      {!dot && (
        <rect
          x={2}
          y={2}
          width={20}
          height={20}
          rx={frameRx}
          fill="none"
          stroke="currentColor"
          strokeWidth={2.5}
        />
      )}
      <rect
        x={dot ? 6 : 8}
        y={dot ? 6 : 8}
        width={dot ? 12 : 8}
        height={dot ? 12 : 8}
        rx={dot ? frameRx * 0.6 : dotRx}
        fill="currentColor"
      />
    </svg>
  );
}

/** A 32px shape-preset cell — mirrors the colors panel's preset buttons (soft-primary when active). */
function ShapeCell({
  selected,
  ariaLabel,
  title,
  onClick,
  children,
}: {
  selected: boolean;
  ariaLabel: string;
  title: string;
  onClick: () => void;
  children: ReactNode;
}) {
  return (
    <button
      type="button"
      aria-pressed={selected}
      aria-label={ariaLabel}
      title={title}
      onClick={onClick}
      className={cn(
        "inline-flex h-8 w-8 items-center justify-center rounded-md border transition-colors",
        "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring",
        selected
          ? "border-primary bg-primary/10 text-primary"
          : "border-border bg-background text-muted-foreground hover:border-border-strong hover:text-foreground",
      )}
    >
      {children}
    </button>
  );
}

/** A muted sub-group label for one eye column. */
function EyeColumn({
  label,
  value,
  ariaPrefix,
  dot,
  onChange,
}: {
  label: string;
  value: FinderShape;
  ariaPrefix: string;
  dot?: boolean;
  onChange: (shape: FinderShape) => void;
}) {
  return (
    <div className="min-w-0 flex-1">
      <div className="mb-1.5 text-xs text-muted-foreground">{label}</div>
      <div className="flex flex-wrap gap-2">
        {FINDER_ORDER.map((shape) => (
          <ShapeCell
            key={shape}
            selected={value === shape}
            ariaLabel={`${ariaPrefix}: ${FINDER_LABEL[shape]}`}
            title={FINDER_LABEL[shape]}
            onClick={() => onChange(shape)}
          >
            <FinderSwatch shape={shape} dot={dot} />
          </ShapeCell>
        ))}
      </div>
    </div>
  );
}

/**
 * Code-styling controls — Variant D layout: a full-width body-module grid above a
 * paired "Eyes" block (External + Internal side by side, hairline between). 32px
 * preset cells matching the colors panel. Drives the preview `style`:
 * `moduleShape` / `finderShape` (outer eye) / `finderDotShape` (inner eye).
 */
export function ShapeControls({
  moduleShape,
  finderShape,
  finderDotShape,
  onModuleShapeChange,
  onFinderShapeChange,
  onFinderDotShapeChange,
}: ShapeControlsProps) {
  return (
    <Stack gap="2">
      <div>
        <div className="mb-2 text-sm text-muted-foreground">Body</div>
        <div className="flex flex-wrap gap-2">
          {MODULE_ORDER.map((shape) => (
            <ShapeCell
              key={shape}
              selected={moduleShape === shape}
              ariaLabel={`Body shape: ${MODULE_LABEL[shape]}`}
              title={MODULE_LABEL[shape]}
              onClick={() => onModuleShapeChange(shape)}
            >
              <ModuleSwatch shape={shape} />
            </ShapeCell>
          ))}
        </div>
      </div>

      <div>
        <div className="mb-2 text-sm text-muted-foreground">Eyes</div>
        <div className="flex items-start gap-3">
          <EyeColumn
            label="External"
            value={finderShape}
            ariaPrefix="External eye"
            onChange={onFinderShapeChange}
          />
          <div className="w-px self-stretch bg-border" />
          <EyeColumn
            label="Internal"
            value={finderDotShape}
            ariaPrefix="Internal eye"
            dot
            onChange={onFinderDotShapeChange}
          />
        </div>
      </div>
    </Stack>
  );
}
