import { OptionTile, OptionTileGroup } from "@wow-two-beta/ui/presentation/actions";
import { ControlGroup } from "@wow-two-beta/ui/presentation/layout";

import { CenterEmojiSizeDisplays } from "./emojiPicker";

/** The largest preview glyph, in px, that still fits inside an `OptionTile` without clipping its frame. */
const MaxPreviewGlyph = 24;

/** Defines props for the center-emoji size control. */
export interface EmojiSizeControlProps {
  /** The emoji rendered inside each tile, so the choice previews the real glyph at that size. */
  readonly char: string;

  /** The current size ratio. */
  readonly sizeRatio: number;

  /** Emits the next size ratio. */
  readonly onChange: (ratio: number) => void;
}

/** Renders the center-emoji size as a small tile set — each tile previews the emoji at that size, no numbers. */
export function EmojiSizeControl({ char, sizeRatio, onChange }: EmojiSizeControlProps) {
  return (
    <ControlGroup label="Size" orientation="vertical" divided={false}>
      <OptionTileGroup label="Emoji size">
        {Object.values(CenterEmojiSizeDisplays).map((display) => (
          <OptionTile
            key={display.label}
            selected={Math.abs(sizeRatio - display.ratio) < 0.001}
            label={`Size: ${display.label}`}
            onSelect={() => onChange(display.ratio)}
          >
            <span
              className="flex h-full w-full items-center justify-center"
              style={{ fontSize: Math.min(display.glyph, MaxPreviewGlyph), lineHeight: 1 }}
            >
              {char}
            </span>
          </OptionTile>
        ))}
      </OptionTileGroup>
    </ControlGroup>
  );
}
