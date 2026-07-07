import { ChevronDown, ChevronRight } from "lucide-react";

import { type CSSProperties } from "react";

import { type EmojiCatalogEntry } from "@/domain/codes/core";

import { EmojiGrid } from "./EmojiGrid";
import {
  CategoryDisplays,
  CategoryOrder,
  type CategoryKey,
  EmojiEmptyLabels,
  type EmojiPickerSize,
  EmojiPickerSizes,
  type EmojiTileShape,
  RecentCategory,
} from "./emojiPicker";

/** Defines props for the collapsible-category emoji layout. */
export interface EmojiAccordionProps {
  /** The active (expanded) category. */
  readonly active: CategoryKey;

  /** Expands a category. */
  readonly onSelectCategory: (category: CategoryKey) => void;

  /** The emoji under the active category (or the search results while searching). */
  readonly visibleEmojis: readonly EmojiCatalogEntry[];

  /** Whether a search is overriding the category sections. */
  readonly showSearchResults: boolean;

  /** The selected emoji's char, or `null`. */
  readonly selectedChar: string | null;

  /** The tile scale. */
  readonly size: EmojiPickerSize;

  /** The tile frame — rounded chip, circle, or borderless. */
  readonly shape: EmojiTileShape;

  /** The fixed viewport height, in tile rows, keeping the surface constant across categories. */
  readonly viewportRows: number;

  /** Picks an emoji. */
  readonly onSelect: (entry: EmojiCatalogEntry) => void;
}

/** Renders emoji categories as a vertical accordion — the open section shows its grid; a search flattens to results. */
export function EmojiAccordion({
  active,
  onSelectCategory,
  visibleEmojis,
  showSearchResults,
  selectedChar,
  size,
  shape,
  viewportRows,
  onSelect,
}: EmojiAccordionProps) {
  if (showSearchResults) {
    return (
      <EmojiGrid
        emojis={visibleEmojis}
        selectedChar={selectedChar}
        size={size}
        shape={shape}
        onSelect={onSelect}
        emptyLabel={EmojiEmptyLabels.search}
        viewportRows={viewportRows}
      />
    );
  }

  const { tile, gap } = EmojiPickerSizes[size];
  const viewportStyle: CSSProperties = {
    height: viewportRows * (tile + gap),
    overflowY: "auto",
    scrollbarGutter: "stable",
  };

  return (
    <div className="flex flex-col" style={viewportStyle}>
      {CategoryOrder.map((key) => {
        const isOpen = key === active;
        const { label, Icon } = CategoryDisplays[key];
        const Chevron = isOpen ? ChevronDown : ChevronRight;

        return (
          <div key={key}>
            <button
              type="button"
              aria-expanded={isOpen}
              onClick={() => onSelectCategory(key)}
              className="flex w-full items-center gap-2 py-2 text-xs font-medium text-muted-foreground hover:text-foreground"
            >
              <Chevron size={14} aria-hidden />
              <Icon size={14} aria-hidden />
              {label}
            </button>
            {isOpen ? (
              <div className="pb-2">
                <EmojiGrid
                  emojis={visibleEmojis}
                  selectedChar={selectedChar}
                  size={size}
                  shape={shape}
                  onSelect={onSelect}
                  emptyLabel={key === RecentCategory ? EmojiEmptyLabels.recents : EmojiEmptyLabels.category}
                />
              </div>
            ) : null}
          </div>
        );
      })}
    </div>
  );
}
