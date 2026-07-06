import type { ReactNode } from "react";
import { cn } from "@wow-two-beta/ui/foundation/utils";
import { Stack } from "@wow-two-beta/ui/presentation/layout";
import { FinderShape, ModuleShape } from "@/domain/codes/core";
import { FinderShapeDisplays, ModuleShapeDisplays } from "./ShapeDisplays";

export interface ShapeControlsProps {
  readonly moduleShape: ModuleShape;
  readonly finderShape: FinderShape;
  readonly finderDotShape: FinderShape;
  readonly onModuleShapeChange: (shape: ModuleShape) => void;
  readonly onFinderShapeChange: (shape: FinderShape) => void;
  readonly onFinderDotShapeChange: (shape: FinderShape) => void;
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
        {(Object.keys(FinderShapeDisplays) as FinderShape[]).map((shape) => {
          const display = FinderShapeDisplays[shape];
          return (
            <ShapeCell
              key={shape}
              selected={value === shape}
              ariaLabel={`${ariaPrefix}: ${display.label}`}
              title={display.label}
              onClick={() => onChange(shape)}
            >
              {dot ? display.dotIcon : display.icon}
            </ShapeCell>
          );
        })}
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
          {(Object.keys(ModuleShapeDisplays) as ModuleShape[]).map((shape) => {
            const display = ModuleShapeDisplays[shape];
            return (
              <ShapeCell
                key={shape}
                selected={moduleShape === shape}
                ariaLabel={`Body shape: ${display.label}`}
                title={display.label}
                onClick={() => onModuleShapeChange(shape)}
              >
                {display.icon}
              </ShapeCell>
            );
          })}
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
