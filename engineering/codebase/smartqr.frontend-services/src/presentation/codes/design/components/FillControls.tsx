import { Fragment } from "react";
import { ArrowLeftRight, ArrowRight } from "lucide-react";

import { ColorTone, SizePreset } from "@wow-two-beta/ui/foundation/utils";
import { Button, ButtonVariant, OptionTile, OptionTileGroup, ToggleButton, ToggleButtonGroup, ToggleButtonGroupVariant, ToggleMode } from "@wow-two-beta/ui/presentation/actions";
import { Separator } from "@wow-two-beta/ui/presentation/display";
import { ColorPicker } from "@wow-two-beta/ui/presentation/forms";
import { ControlGroup, Stack } from "@wow-two-beta/ui/presentation/layout";

import { FillType, Gradient, GradientType } from "@/domain/codes/style";

import { PresetIconSize, PresetRows } from "./GradientPresets";

/** @internal Default linear-gradient angle (deg). */
const DefaultAngle = 45;

/** @internal Default radial-gradient radius (0..1 extent). */
const DefaultRadius = 0.8;

/** @internal Default gradient end stop — the brand violet (matches `--color-primary`). A stored data value, so a concrete hex, not a CSS token. */
const DefaultGradientEnd = "#7c3aed";

/** Resolves the gradient's current projection value — its angle (linear) or radius (radial). */
function projectionValue(gradient: Gradient): number {
  return gradient.type === GradientType.Linear ? gradient.angle : gradient.radius;
}

/** Applies a projection value to the gradient — its angle (linear) or radius (radial). */
function applyProjection(gradient: Gradient, value: number): Gradient {
  return gradient.type === GradientType.Linear ? Gradient.withAngle(gradient, value) : Gradient.withRadius(gradient, value);
}

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
    if (next === FillType.Gradient) onGradientChange(Gradient.twoStop(foreground, DefaultGradientEnd, DefaultAngle));
    else if (next === FillType.Solid) onGradientChange(null);
  }

  // Switch the gradient's projection (linear ↔ radial), seeding the target variant's field.
  function changeType(next: GradientType) {
    if (!gradient || gradient.type === next) return;
    onGradientChange(Gradient.withType(gradient, next, { angle: DefaultAngle, radius: DefaultRadius }));
  }

  return (
    <Stack gap="0">
      {/* Fill mode — solid vs two-stop gradient. */}
      <ControlGroup label="Fill">
        <ToggleButtonGroup<FillType>
          variant={ToggleButtonGroupVariant.Segmented}
          type={ToggleMode.Single}
          value={gradient === null ? FillType.Solid : FillType.Gradient}
          onValueChange={setFill}
          aria-label="Foreground fill"
        >
          <ToggleButton value={FillType.Solid} size={SizePreset.Sm}>Solid</ToggleButton>
          <ToggleButton value={FillType.Gradient} size={SizePreset.Sm}>Gradient</ToggleButton>
        </ToggleButtonGroup>
      </ControlGroup>

      {/* Colors — the gradient stops, or the solid foreground. */}
      <ControlGroup label="Colors">
        {gradient ? (
          <>
            <ColorPicker
              triggerVariant="swatch"
              value={gradient.stops[0]?.color ?? DefaultGradientEnd}
              onValueChange={(hex) => onGradientChange(Gradient.withStop(gradient, 0, hex))}
              aria-label="Gradient start color"
            />
            <Button
              variant={ButtonVariant.Reveal}
              tone={ColorTone.Neutral}
              shape="square"
              size={SizePreset.Xs}
              aria-label="Reverse gradient colors"
              onClick={() => onGradientChange(Gradient.reverseStops(gradient))}
              hoverSlot={<ArrowLeftRight size={PresetIconSize} />}
            >
              <ArrowRight size={PresetIconSize} />
            </Button>
            <ColorPicker
              triggerVariant="swatch"
              value={gradient.stops[1]?.color ?? DefaultGradientEnd}
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
            variant={ToggleButtonGroupVariant.Segmented}
            type={ToggleMode.Single}
            orientation="vertical"
            value={gradient.type}
            onValueChange={(v) => v && changeType(v)}
            aria-label="Gradient type"
            className="shrink-0 self-stretch"
          >
            <ToggleButton value={GradientType.Linear} size={SizePreset.Sm} className="flex-1">
              Linear
            </ToggleButton>
            <ToggleButton value={GradientType.Radial} size={SizePreset.Sm} className="flex-1">
              Radial
            </ToggleButton>
          </ToggleButtonGroup>

          <div className="flex flex-1 flex-col">
            {PresetRows.map((row, index) => (
              <Fragment key={row.type}>
                {index > 0 && <Separator />}
                <OptionTileGroup
                  label={row.ariaLabel}
                  disabled={gradient.type !== row.type}
                  align="end"
                  className="py-2 first:pt-0 last:pb-0"
                >
                  {row.presets.map((preset) => (
                    <OptionTile
                      key={preset.value}
                      selected={gradient.type === row.type && projectionValue(gradient) === preset.value}
                      label={preset.label}
                      onSelect={() => onGradientChange(applyProjection(gradient, preset.value))}
                    >
                      {preset.glyph}
                    </OptionTile>
                  ))}
                </OptionTileGroup>
              </Fragment>
            ))}
          </div>
        </div>
      )}
    </Stack>
  );
}
