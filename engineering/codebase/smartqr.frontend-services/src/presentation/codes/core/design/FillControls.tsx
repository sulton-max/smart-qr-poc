import { Fragment, type ReactNode } from "react";
import { ArrowLeftRight, ArrowRight } from "lucide-react";

import { Button, OptionTile, ToggleButton, ToggleButtonGroup } from "@wow-two-beta/ui/presentation/actions";
import { RadiusGlyph, Separator } from "@wow-two-beta/ui/presentation/display";
import { ColorPicker } from "@wow-two-beta/ui/presentation/forms";
import { ControlGroup, Stack } from "@wow-two-beta/ui/presentation/layout";

import { FillType, Gradient, GradientType } from "@/domain/codes/core";

import { ANGLES, DEFAULT_ANGLE, DEFAULT_GRADIENT_END, DEFAULT_RADIUS, PRESET_ICON_SIZE, RADII } from "./gradient";

/** Defines one selectable preset within a gradient projection row. */
interface GradientPreset {
  /** The value the preset applies — angle in degrees, or radius extent. */
  readonly value: number;

  /** The accessible label for the preset tile. */
  readonly label: string;

  /** The preset's glyph. */
  readonly glyph: ReactNode;
}

/** Defines one projection row — a gradient type's presets plus how they bind to the gradient. */
interface PresetRow {
  /** The gradient type this row drives (also gates its enabled state). */
  readonly type: GradientType;

  /** The row's selectable presets. */
  readonly presets: ReadonlyArray<GradientPreset>;

  /** Whether `value` is the gradient's current selection. */
  readonly isSelected: (gradient: Gradient, value: number) => boolean;

  /** Applies `value` to the gradient. */
  readonly apply: (gradient: Gradient, value: number) => Gradient;
}

/** The projection rows — one per `GradientType`, binding its angle / radius presets to the gradient. */
const PRESET_ROWS: PresetRow[] = [
  {
    type: GradientType.Linear,
    presets: ANGLES.map((angle) => ({
      value: angle.value,
      label: `${angle.label} (${angle.value}°)`,
      glyph: <angle.Icon size={PRESET_ICON_SIZE} />,
    })),
    isSelected: (gradient, value) => gradient.type === GradientType.Linear && gradient.angle === value,
    apply: (gradient, value) => Gradient.withAngle(gradient, value),
  },
  {
    type: GradientType.Radial,
    presets: RADII.map((radius) => ({
      value: radius.value,
      label: `${radius.label} radius`,
      glyph: <RadiusGlyph extent={radius.value} />,
    })),
    isSelected: (gradient, value) => gradient.type === GradientType.Radial && gradient.radius === value,
    apply: (gradient, value) => Gradient.withRadius(gradient, value),
  },
];

/** Defines props for the fill controls. */
export interface FillControlsProps {
  /** The solid foreground — seeds the gradient's first stop when first enabled. */
  readonly foreground: string;

  /** Emits the next solid foreground. */
  readonly onForegroundChange: (hex: string) => void;

  /** The current foreground gradient, or null for a solid foreground. */
  readonly gradient: Gradient | null;

  /** Emits the next gradient, or null to fall back to the solid foreground. */
  readonly onGradientChange: (gradient: Gradient | null) => void;
}

/** Renders the foreground and background fill controls. */
export function FillControls({ foreground, onForegroundChange, gradient, onGradientChange }: FillControlsProps) {
  // Toggle between a solid foreground and a default two-stop gradient.
  function setFill(next: FillType | null) {
    if (next === FillType.Gradient) onGradientChange(Gradient.twoStop(foreground, DEFAULT_GRADIENT_END, DEFAULT_ANGLE));
    else if (next === FillType.Solid) onGradientChange(null);
  }

  // Switch the gradient's projection (linear ↔ radial), seeding the target variant's field.
  function changeType(next: GradientType) {
    if (!gradient || gradient.type === next) return;
    onGradientChange(Gradient.withType(gradient, next, { angle: DEFAULT_ANGLE, radius: DEFAULT_RADIUS }));
  }

  return (
    <Stack gap="0">
      {/* Fill mode — solid vs two-stop gradient. */}
      <ControlGroup label="Fill">
        <ToggleButtonGroup<FillType>
          variant="segmented"
          type="single"
          value={gradient === null ? FillType.Solid : FillType.Gradient}
          onValueChange={setFill}
          aria-label="Foreground fill"
        >
          <ToggleButton value={FillType.Solid} size="sm">Solid</ToggleButton>
          <ToggleButton value={FillType.Gradient} size="sm">Gradient</ToggleButton>
        </ToggleButtonGroup>
      </ControlGroup>

      {/* Colors — the gradient stops, or the solid foreground. */}
      <ControlGroup label="Colors">
        {gradient ? (
          <>
            <ColorPicker
              triggerVariant="swatch"
              value={gradient.stops[0]?.color ?? DEFAULT_GRADIENT_END}
              onValueChange={(hex) => onGradientChange(Gradient.withStop(gradient, 0, hex))}
              aria-label="Gradient start color"
            />
            <Button
              variant="reveal"
              tone="neutral"
              shape="square"
              size="xs"
              aria-label="Reverse gradient colors"
              onClick={() => onGradientChange(Gradient.reverseStops(gradient))}
              hoverSlot={<ArrowLeftRight size={PRESET_ICON_SIZE} />}
            >
              <ArrowRight size={PRESET_ICON_SIZE} />
            </Button>
            <ColorPicker
              triggerVariant="swatch"
              value={gradient.stops[1]?.color ?? DEFAULT_GRADIENT_END}
              onValueChange={(hex) => onGradientChange(Gradient.withStop(gradient, 1, hex))}
              aria-label="Gradient end color"
            />
          </>
        ) : (
          <ColorPicker
            triggerVariant="swatch"
            value={foreground}
            onValueChange={onForegroundChange}
            aria-label="Foreground color"
          />
        )}
      </ControlGroup>

      {/* Gradient projection — linear angle / radial radius presets. */}
      {gradient && (
        <div className="flex items-stretch gap-3 py-2">
          <ToggleButtonGroup<GradientType>
            variant="segmented"
            type="single"
            orientation="vertical"
            value={gradient.type}
            onValueChange={(v) => v && changeType(v)}
            aria-label="Gradient type"
            className="shrink-0 self-stretch"
          >
            <ToggleButton value={GradientType.Linear} size="sm" className="flex-1">
              Linear
            </ToggleButton>
            <ToggleButton value={GradientType.Radial} size="sm" className="flex-1">
              Radial
            </ToggleButton>
          </ToggleButtonGroup>

          <div className="flex flex-1 flex-col">
            {PRESET_ROWS.map((row, index) => (
              <Fragment key={row.type}>
                {index > 0 && <Separator />}
                <fieldset
                  disabled={gradient.type !== row.type}
                  className="flex min-w-0 items-center justify-end gap-2 py-2 first:pt-0 last:pb-0"
                >
                  {row.presets.map((preset) => (
                    <OptionTile
                      key={preset.value}
                      selected={row.isSelected(gradient, preset.value)}
                      label={preset.label}
                      onSelect={() => onGradientChange(row.apply(gradient, preset.value))}
                    >
                      {preset.glyph}
                    </OptionTile>
                  ))}
                </fieldset>
              </Fragment>
            ))}
          </div>
        </div>
      )}
    </Stack>
  );
}
