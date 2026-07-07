import { useCallback, useMemo, useState } from "react";

import { type EmojiCatalogEntry, emojisByCategory, type DesignEmojiOverlay, searchEmoji } from "@/domain/codes/core";
import { useRecentItems } from "@/application/common/storage";

import { type CategoryKey, DefaultEmojiSize, EmojiRecentsKey, RecentCategory } from "./emojiPicker";

/** Defines the inputs the picker view-model binds to the live preview value. */
export interface UseEmojiPickerOptions {
  /** The current center emoji, or `null` for none. */
  readonly value: DesignEmojiOverlay | null;

  /** Emits the next center emoji, or `null` to clear it. */
  readonly onChange: (emoji: DesignEmojiOverlay | null) => void;
}

/** Represents the headless picker state — layout-agnostic, shared by every nav variant. */
export interface EmojiPickerModel {
  /** The current search keyword. */
  readonly searchKeyword: string;

  /** Sets the search keyword. */
  readonly setSearchKeyword: (keyword: string) => void;

  /** The active category (or the synthetic recents bucket). */
  readonly activeCategory: CategoryKey;

  /** Selects the active category. */
  readonly setActiveCategory: (category: CategoryKey) => void;

  /** Whether a non-empty keyword is driving the visible list, bypassing categories. */
  readonly showSearchResults: boolean;

  /** The emoji to render right now — search results, recents, or the active category. */
  readonly visibleEmojis: readonly EmojiCatalogEntry[];

  /** The most-recently-used emoji, most-recent-first. */
  readonly recents: readonly EmojiCatalogEntry[];

  /** The selected center emoji, or `null` for none. */
  readonly selected: DesignEmojiOverlay | null;

  /** Picks an emoji — updates the preview (preserving any prior size) and records it as recent. */
  readonly selectEmoji: (entry: EmojiCatalogEntry) => void;

  /** Clears the center emoji. */
  readonly clearSelection: () => void;
}

/**
 * Manages the emoji picker independent of layout: owns the keyword, the active category, the derived
 * visible list, and the recents MRU — and folds a pick back into the preview value + the recents list.
 */
export function useEmojiPicker({ value, onChange }: UseEmojiPickerOptions): EmojiPickerModel {
  const { recents, push } = useRecentItems<EmojiCatalogEntry>(EmojiRecentsKey, {
    identify: (entry) => entry.char,
  });

  const [searchKeyword, setSearchKeyword] = useState("");
  const [activeCategory, setActiveCategory] = useState<CategoryKey>(RecentCategory);

  const trimmedKeyword = searchKeyword.trim();
  const showSearchResults = trimmedKeyword.length > 0;

  const visibleEmojis = useMemo<readonly EmojiCatalogEntry[]>(() => {
    if (showSearchResults) return searchEmoji(trimmedKeyword);
    if (activeCategory === RecentCategory) return recents;
    return emojisByCategory(activeCategory);
  }, [showSearchResults, trimmedKeyword, activeCategory, recents]);

  const selectEmoji = useCallback(
    (entry: EmojiCatalogEntry) => {
      onChange({ char: entry.char, sizeRatio: value?.sizeRatio ?? DefaultEmojiSize });
      push(entry);
    },
    [onChange, value?.sizeRatio, push],
  );

  const clearSelection = useCallback(() => onChange(null), [onChange]);

  return useMemo<EmojiPickerModel>(
    () => ({
      searchKeyword,
      setSearchKeyword,
      activeCategory,
      setActiveCategory,
      showSearchResults,
      visibleEmojis,
      recents,
      selected: value,
      selectEmoji,
      clearSelection,
    }),
    [searchKeyword, activeCategory, showSearchResults, visibleEmojis, recents, value, selectEmoji, clearSelection],
  );
}
