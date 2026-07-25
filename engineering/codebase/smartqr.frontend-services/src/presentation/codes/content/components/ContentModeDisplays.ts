import { BadgeVariant } from "@wow-two-beta/ui/presentation/display";
import { ContentMode } from "@/domain/codes/content";

/** Defines the display for a content mode — the choice is permanent, so the copy states what each one costs. */
interface ContentModeDisplay {
  /** The short label — the mode picker and the code card's chip. */
  label: string;

  /** The one-line trade-off shown under the picker. */
  note: string;

  /** The chip treatment on the code card. */
  badge: BadgeVariant;
}

/** Maps each content mode to its label, trade-off note, and card-chip treatment. */
export const ContentModeDisplays: Record<ContentMode, ContentModeDisplay> = {
  [ContentMode.Static]: {
    label: "Static",
    note: "The code carries the content itself — it works offline, and changing the content makes a different code.",
    // Outline, not neutral — a neutral fill matches the card surface and stops reading as a chip.
    badge: BadgeVariant.Outline,
  },
  [ContentMode.Dynamic]: {
    label: "Dynamic",
    note: "The code carries a short link — edit the content any time without reprinting.",
    badge: BadgeVariant.Brand,
  },
};
