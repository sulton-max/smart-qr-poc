import { type CSSProperties } from "react";

import { type EmojiCatalogEntry } from "@/domain/codes/core";

import { type EmojiPickerSize, EmojiPickerSizes, EmojiTileShape } from "./emojiPicker";

/** Maps each tile shape to its frame classes (border + corner); state + hover wash layer on top. */
const TileFrames: Record<EmojiTileShape, string> = {
  [EmojiTileShape.Rounded]: "rounded-md border",
  [EmojiTileShape.Circle]: "rounded-full border",
  [EmojiTileShape.Plain]: "rounded-md",
};

/** Defines props for a single selectable emoji tile. */
interface EmojiTileProps {
  /** The catalog emoji this tile renders. */
  readonly entry: EmojiCatalogEntry;

  /** Whether this tile is the active selection. */
  readonly selected: boolean;

  /** The tile scale. */
  readonly size: EmojiPickerSize;

  /** The tile frame — rounded chip, circle, or borderless. */
  readonly shape: EmojiTileShape;

  /** Selects this emoji. */
  readonly onSelect: (entry: EmojiCatalogEntry) => void;
}

/** Renders one selectable emoji tile — its glyph sized by the picker scale, framed by the tile shape. */
function EmojiTile({ entry, selected, size, shape, onSelect }: EmojiTileProps) {
  const { tile, glyph } = EmojiPickerSizes[size];
  const isCircle = shape === EmojiTileShape.Circle;

  const stateClass = selected
    ? shape === EmojiTileShape.Plain
      ? "bg-primary/15"
      : "border-primary bg-primary/10"
    : shape === EmojiTileShape.Plain
      ? "hover:bg-muted/60"
      : "border-transparent hover:bg-muted";

  return (
    <button
      type="button"
      title={entry.label}
      aria-label={entry.label}
      aria-pressed={selected}
      onClick={() => onSelect(entry)}
      className={`flex items-center justify-center leading-none transition-colors ${TileFrames[shape]} ${stateClass} ${
        isCircle ? "place-self-center" : ""
      }`}
      style={isCircle ? { height: tile, width: tile, fontSize: glyph } : { height: tile, fontSize: glyph }}
    >
      {entry.char}
    </button>
  );
}

/** Defines props for the emoji tile grid. */
export interface EmojiGridProps {
  /** The emoji to lay out, in order. */
  readonly emojis: readonly EmojiCatalogEntry[];

  /** The selected emoji's char, or `null`. */
  readonly selectedChar: string | null;

  /** The tile scale. */
  readonly size: EmojiPickerSize;

  /** The tile frame — rounded chip, circle, or borderless. */
  readonly shape: EmojiTileShape;

  /** Selects an emoji. */
  readonly onSelect: (entry: EmojiCatalogEntry) => void;

  /** The muted hint shown when `emojis` is empty. */
  readonly emptyLabel?: string;

  /** The fixed viewport height, in tile rows; when set, the grid scrolls inside a constant-height box. */
  readonly viewportRows?: number;
}

/** Renders emoji as an auto-filling tile grid, or a muted hint when the set is empty. */
export function EmojiGrid({ emojis, selectedChar, size, shape, onSelect, emptyLabel, viewportRows }: EmojiGridProps) {
  const { tile, gap } = EmojiPickerSizes[size];

  const viewportStyle: CSSProperties | undefined =
    viewportRows === undefined
      ? undefined
      : { height: viewportRows * (tile + gap), overflowY: "auto", scrollbarGutter: "stable" };

  const body =
    emojis.length === 0 ? (
      <p className="py-6 text-center text-xs text-muted-foreground">{emptyLabel ?? "No emoji here yet."}</p>
    ) : (
      <div
        style={{
          display: "grid",
          gridTemplateColumns: `repeat(auto-fill, minmax(${tile}px, 1fr))`,
          gap,
        }}
      >
        {emojis.map((entry) => (
          <EmojiTile
            key={entry.char}
            entry={entry}
            selected={entry.char === selectedChar}
            size={size}
            shape={shape}
            onSelect={onSelect}
          />
        ))}
      </div>
    );

  return viewportStyle === undefined ? body : <div style={viewportStyle}>{body}</div>;
}
