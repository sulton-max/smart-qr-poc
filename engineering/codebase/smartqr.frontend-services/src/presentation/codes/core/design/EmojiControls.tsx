import { Button } from "@wow-two-beta/ui/presentation/actions";
import { SearchInput } from "@wow-two-beta/ui/presentation/forms";
import { Stack } from "@wow-two-beta/ui/presentation/layout";

import { type DesignEmojiOverlay } from "@/domain/codes/core";

import {
  CategoryNav,
  CategoryNavVariant,
  DefaultPickerSize,
  EmojiAccordion,
  EmojiEmptyLabels,
  EmojiGrid,
  type EmojiPickerSize,
  EmojiSizeControl,
  EmojiTileShape,
  RecentCategory,
  useEmojiPicker,
} from "./emoji";

/** Defines props for the center-emoji picker. */
export interface EmojiControlsProps {
  /** The current center emoji, or `null` for none. */
  readonly emoji: DesignEmojiOverlay | null;

  /** Emits the next center emoji, or `null` to clear it. */
  readonly onChange: (emoji: DesignEmojiOverlay | null) => void;

  /** The category-navigation affordance. Default `strip`. */
  readonly categoryNavVariant?: CategoryNavVariant;

  /** The element scale — one value for every element, or a per-element `{ search, nav, tile }`. Default `md`. */
  readonly size?: EmojiPickerSize | { search?: EmojiPickerSize; nav?: EmojiPickerSize; tile?: EmojiPickerSize };

  /** The emoji-tile frame — rounded chip, circle, or borderless. Default `rounded`. */
  readonly tileShape?: EmojiTileShape;

  /** The scrollable tile viewport's height, in tile rows. Default `6`. */
  readonly rowsCount?: number;
}

/** Resolves the `size` prop to a per-element scale, falling back to `DefaultPickerSize`. */
function resolveSizes(size: EmojiControlsProps["size"]): {
  search: EmojiPickerSize;
  nav: EmojiPickerSize;
  tile: EmojiPickerSize;
} {
  if (typeof size === "string") return { search: size, nav: size, tile: size };
  return {
    search: size?.search ?? DefaultPickerSize,
    nav: size?.nav ?? DefaultPickerSize,
    tile: size?.tile ?? DefaultPickerSize,
  };
}

/**
 * Center-emoji picker (v0.5) — search + recents + a swappable category nav over the full emoji set. `null` = none.
 * Rides the live preview `style.emoji`; the backend bumps ECC to H so the code still scans.
 */
export function EmojiControls({
  emoji,
  onChange,
  categoryNavVariant = CategoryNavVariant.Strip,
  size,
  tileShape = EmojiTileShape.Rounded,
  rowsCount = 6,
}: EmojiControlsProps) {
  const picker = useEmojiPicker({ value: emoji, onChange });

  const { search, nav, tile } = resolveSizes(size);
  const selectedChar = picker.selected?.char ?? null;

  return (
    <Stack gap="3">
      <div className="flex items-center justify-between">
        <span className="text-sm font-medium">Center emoji</span>
        {/* TODO(sdk): Button `variant`/`tone` string literals — enum-ify once the SDK ships the tokens. */}
        <Button
          variant={emoji === null ? undefined : "outline"}
          tone={emoji === null ? "primary" : "neutral"}
          size="sm"
          aria-pressed={emoji === null}
          onClick={picker.clearSelection}
        >
          None
        </Button>
      </div>

      <SearchInput
        size={search}
        placeholder="Search emoji…"
        value={picker.searchKeyword}
        onChange={(event) => picker.setSearchKeyword(event.target.value)}
        onClear={() => picker.setSearchKeyword("")}
      />

      {categoryNavVariant === CategoryNavVariant.Accordion ? (
        <EmojiAccordion
          active={picker.activeCategory}
          onSelectCategory={picker.setActiveCategory}
          visibleEmojis={picker.visibleEmojis}
          showSearchResults={picker.showSearchResults}
          selectedChar={selectedChar}
          size={tile}
          shape={tileShape}
          viewportRows={rowsCount}
          onSelect={picker.selectEmoji}
        />
      ) : (
        <Stack gap="2">
          {picker.showSearchResults ? null : (
            <CategoryNav
              variant={categoryNavVariant}
              active={picker.activeCategory}
              onSelect={picker.setActiveCategory}
              size={nav}
            />
          )}
          <EmojiGrid
            emojis={picker.visibleEmojis}
            selectedChar={selectedChar}
            size={tile}
            shape={tileShape}
            onSelect={picker.selectEmoji}
            viewportRows={rowsCount}
            emptyLabel={
              picker.showSearchResults
                ? EmojiEmptyLabels.search
                : picker.activeCategory === RecentCategory
                  ? EmojiEmptyLabels.recents
                  : EmojiEmptyLabels.category
            }
          />
        </Stack>
      )}

      {emoji === null ? null : (
        <EmojiSizeControl
          char={emoji.char}
          sizeRatio={emoji.sizeRatio}
          onChange={(ratio) => onChange({ ...emoji, sizeRatio: ratio })}
        />
      )}
    </Stack>
  );
}
