import type { EmojiCategory } from "./EmojiCategory";
import { emojiCatalogData } from "./EmojiCatalogData";

/** Represents one catalog emoji: its char, display label, search tags, and display category. */
export interface EmojiCatalogEntry {
  char: string;
  label: string;
  tags: readonly string[];
  category: EmojiCategory;
}

/** The full standard emoji set (base emoji only, no skin-tone variants), in display order. */
export const EmojiCatalog: readonly EmojiCatalogEntry[] = emojiCatalogData;

/** Returns every catalog emoji in the given category, preserving display order. */
export function emojisByCategory(category: EmojiCategory): readonly EmojiCatalogEntry[] {
  return EmojiCatalog.filter((entry) => entry.category === category);
}

/**
 * Searches the catalog by a free-text query, case-insensitively.
 * Matches on label and tags; ranks label matches above tag-only matches, and a
 * label-prefix hit above a label-substring hit. Ties keep display order. A blank
 * query returns the whole catalog.
 */
export function searchEmoji(query: string): readonly EmojiCatalogEntry[] {
  const needle = query.trim().toLowerCase();
  if (!needle) return EmojiCatalog;

  const ranked: { entry: EmojiCatalogEntry; rank: number; order: number }[] = [];
  for (let order = 0; order < EmojiCatalog.length; order++) {
    const entry = EmojiCatalog[order];
    const label = entry.label.toLowerCase();

    let rank: number;
    if (label.startsWith(needle)) rank = 0;
    else if (label.includes(needle)) rank = 1;
    else if (entry.tags.some((tag) => tag.includes(needle))) rank = 2;
    else continue;

    ranked.push({ entry, rank, order });
  }

  ranked.sort((a, b) => a.rank - b.rank || a.order - b.order);
  return ranked.map((hit) => hit.entry);
}
