import { ColorPicker } from "@wow-two-beta/ui/forms";
import { cn } from "@wow-two-beta/ui/utils";

export interface TileColorPickerProps {
  /** Current color (`#RRGGBB`). */
  value: string;
  /** Emit the next color. */
  onValueChange: (hex: string) => void;
  /** Accessible label for the swatch trigger. */
  ariaLabel: string;
  /** Swatch size. Default `md`. */
  size?: "sm" | "md" | "lg";
}

const TILE_SIZE: Record<NonNullable<TileColorPickerProps["size"]>, string> = {
  sm: "h-5 w-5",
  md: "h-6 w-6",
  lg: "h-8 w-8",
};

/**
 * Tile-only color trigger — a bare swatch button that opens the SDK `ColorPicker`.
 * Uses the ColorPicker `trigger` slot (replaces the default frame + hex-value text), so the
 * swatch needs no CSS overrides.
 */
export function TileColorPicker({ value, onValueChange, ariaLabel, size = "md" }: TileColorPickerProps) {
  return (
    <ColorPicker
      value={value}
      onValueChange={(hex) => onValueChange(hex ?? value)}
      trigger={
        <button
          type="button"
          aria-label={ariaLabel}
          className={cn(
            "inline-block shrink-0 rounded-md border border-border transition-colors hover:border-border-strong focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring",
            TILE_SIZE[size],
          )}
          style={{ backgroundColor: value }}
        />
      }
    />
  );
}
