import { Bike, Car, Clock, Flag, Hash, Lightbulb, type LucideIcon, PawPrint, Smile, Utensils } from "lucide-react";

import { type SizePreset } from "@wow-two-beta/ui/foundation/utils";

import { EmojiCategory } from "@/domain/codes/core";

/** A uniform t-shirt scale, applied per picker element (search, category nav, tiles). */
export type EmojiPickerSize = Extract<SizePreset, "sm" | "md" | "lg">;

/** The picker's element scale applied wherever no per-element size is given. */
// TODO(sdk): source from a SizePreset enum
export const DefaultPickerSize: EmojiPickerSize = "md";

/** Defines which category-navigation affordance the picker renders. */
export const CategoryNavVariant = {
  /** Refers to the segmented icon-strip layout. */
  Strip: "strip",
  /** Refers to the labelled pill-row layout. */
  Pills: "pills",
  /** Refers to the vertical collapsible-accordion layout. */
  Accordion: "accordion",
} as const;

export type CategoryNavVariant = (typeof CategoryNavVariant)[keyof typeof CategoryNavVariant];

/** Defines how each emoji tile is framed — a rounded-rect chip, a circle, or borderless (transparent). */
export const EmojiTileShape = {
  /** Refers to a rounded-rect chip frame. */
  Rounded: "rounded",
  /** Refers to a circular frame. */
  Circle: "circle",
  /** Refers to a borderless (transparent) frame. */
  Plain: "plain",
} as const;

export type EmojiTileShape = (typeof EmojiTileShape)[keyof typeof EmojiTileShape];

/** Defines a discrete center-emoji size preset — the ratio it applies to the code width, plus its preview glyph. */
export const CenterEmojiSize = {
  /** Refers to the small center-emoji size. */
  Small: "small",
  /** Refers to the medium center-emoji size. */
  Medium: "medium",
  /** Refers to the large center-emoji size. */
  Large: "large",
} as const;

export type CenterEmojiSize = (typeof CenterEmojiSize)[keyof typeof CenterEmojiSize];

/** Defines the display for a center-emoji size option. */
interface CenterEmojiSizeDisplay {
  label: string;

  ratio: number;

  glyph: number;
}

/** Maps each center-emoji size to its label, code-width ratio, and preview glyph size. */
export const CenterEmojiSizeDisplays: Record<CenterEmojiSize, CenterEmojiSizeDisplay> = {
  [CenterEmojiSize.Small]: { label: "Small", ratio: 0.18, glyph: 16 },
  [CenterEmojiSize.Medium]: { label: "Medium", ratio: 0.25, glyph: 22 },
  [CenterEmojiSize.Large]: { label: "Large", ratio: 0.32, glyph: 28 },
};

/** The center-emoji size (fraction of the code's width) applied when a freshly picked emoji brings none. */
export const DefaultEmojiSize = 0.25;

/** The `localStorage` key the picker persists its most-recently-used emoji under. */
export const EmojiRecentsKey = "smartqr.emoji.recents";

/** Defines the pixel geometry an `EmojiPickerSize` maps to for the rolled tile + nav elements. */
export interface EmojiPickerSizeTokens {
  /** The emoji tile's min edge, in px; the grid auto-fills columns at this width. */
  readonly tile: number;

  /** The emoji glyph's font size, in px. */
  readonly glyph: number;

  /** The category-nav control's edge, in px. */
  readonly nav: number;

  /** The inter-tile gap, in px. */
  readonly gap: number;
}

/** Maps each `EmojiPickerSize` to its tile / glyph / nav / gap pixel geometry. */
export const EmojiPickerSizes: Record<EmojiPickerSize, EmojiPickerSizeTokens> = {
  sm: { tile: 32, glyph: 18, nav: 26, gap: 4 },
  md: { tile: 40, glyph: 22, nav: 30, gap: 6 },
  lg: { tile: 48, glyph: 26, nav: 34, gap: 8 },
};

/** The synthetic "recently used" bucket — nav index 0, backed by the MRU list rather than the catalog. */
export const RecentCategory = "recent";

/** A navigable picker section: the synthetic recents bucket or one real catalog category. */
export type CategoryKey = typeof RecentCategory | EmojiCategory;

/** Defines a category's nav presentation — its accessible label and glyph. */
interface CategoryDisplay {
  label: string;

  Icon: LucideIcon;
}

/** The nav categories in display order — recents first, then the catalog's Telegram-order groups. */
export const CategoryOrder: readonly CategoryKey[] = [
  RecentCategory,
  EmojiCategory.SmileysPeople,
  EmojiCategory.AnimalsNature,
  EmojiCategory.FoodDrink,
  EmojiCategory.Activity,
  EmojiCategory.TravelPlaces,
  EmojiCategory.Objects,
  EmojiCategory.Symbols,
  EmojiCategory.Flags,
];

/** Maps each nav category to its label + icon. */
export const CategoryDisplays: Record<CategoryKey, CategoryDisplay> = {
  [RecentCategory]: { label: "Recent", Icon: Clock },
  [EmojiCategory.SmileysPeople]: { label: "Smileys & people", Icon: Smile },
  [EmojiCategory.AnimalsNature]: { label: "Animals & nature", Icon: PawPrint },
  [EmojiCategory.FoodDrink]: { label: "Food & drink", Icon: Utensils },
  [EmojiCategory.Activity]: { label: "Activity", Icon: Bike },
  [EmojiCategory.TravelPlaces]: { label: "Travel & places", Icon: Car },
  [EmojiCategory.Objects]: { label: "Objects", Icon: Lightbulb },
  [EmojiCategory.Symbols]: { label: "Symbols", Icon: Hash },
  [EmojiCategory.Flags]: { label: "Flags", Icon: Flag },
};

/** The muted hints shown when a picker surface has no emoji to render. */
export const EmojiEmptyLabels = {
  /** Refers to the empty search-results hint. */
  search: "No matches.",
  /** Refers to the empty recents hint. */
  recents: "Your picks show up here.",
  /** Refers to the empty category hint. */
  category: "No emoji here yet.",
} as const;
