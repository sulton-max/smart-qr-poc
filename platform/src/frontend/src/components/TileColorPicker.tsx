import { ColorPicker } from "@wow-two-beta/ui/forms";

export interface TileColorPickerProps {
  /** Current color (`#RRGGBB`). */
  value: string;
  /** Emit the next color. */
  onValueChange: (hex: string) => void;
  /** Accessible label for the swatch trigger. */
  ariaLabel: string;
  /** Swatch size — maps to the SDK ColorPicker `triggerSize`. Default `md`. */
  size?: "sm" | "md" | "lg";
}

/**
 * Tile-only color trigger — the SDK `ColorPicker` stripped down to just its swatch chip
 * (the bordered button frame + hex-value text are hidden via arbitrary variants).
 *
 * Interim wrapper: the clean path is a `showValue` prop on the SDK `ColorPicker`
 * (extract this swatch-only mode upstream later).
 */
export function TileColorPicker({ value, onValueChange, ariaLabel, size = "md" }: TileColorPickerProps) {
  return (
    <span className="contents [&_button]:!gap-0 [&_button]:!border-transparent [&_button]:!bg-transparent [&_button]:!px-0 [&_button]:!py-0 [&_.font-mono]:!hidden">
      <ColorPicker
        value={value}
        onValueChange={(hex) => onValueChange(hex ?? value)}
        triggerSize={size}
        aria-label={ariaLabel}
      />
    </span>
  );
}
