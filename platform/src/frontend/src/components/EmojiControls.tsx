import { Button } from "@wow-two-beta/ui/actions";
import { FormField } from "@wow-two-beta/ui/forms";
import { Grid, Stack } from "@wow-two-beta/ui/layout";
import { type PreviewEmoji } from "../types";

// Curated, high-contrast emoji that stay legible shrunk to a code's center.
const EMOJIS = ["🎉", "❤️", "⭐", "🔥", "✅", "📍", "🎁", "☕", "🍕", "🎵", "📱", "💡", "🛒", "🏷️", "👍", "🚀"];
const DEFAULT_SIZE = 0.25;

export interface EmojiControlsProps {
  /** The current center emoji, or `null` for none. */
  emoji: PreviewEmoji | null;
  /** Emit the next emoji, or `null` to clear it. */
  onChange: (emoji: PreviewEmoji | null) => void;
}

/**
 * Center-emoji picker (v0.5) — a brand mark with no file upload. `null` = none.
 * Rides the live preview `style.emoji`; the backend bumps ECC to H so the code still scans.
 */
export function EmojiControls({ emoji, onChange }: EmojiControlsProps) {
  const current = emoji?.char ?? null;

  return (
    <Stack gap="4">
      <FormField
        label="Center emoji"
        helper="A brand mark at the center — no upload needed. Error-correction bumps to H so the code still scans."
      >
        <Grid columns="6" gap="2">
          <Button
            variant={current === null ? undefined : "outline"}
            tone={current === null ? "primary" : "neutral"}
            size="sm"
            aria-pressed={current === null}
            onClick={() => onChange(null)}
          >
            None
          </Button>
          {EMOJIS.map((e) => (
            <Button
              key={e}
              variant={current === e ? undefined : "outline"}
              tone={current === e ? "primary" : "neutral"}
              size="sm"
              className="text-lg"
              aria-label={`Emoji ${e}`}
              aria-pressed={current === e}
              onClick={() => onChange({ char: e, sizeRatio: emoji?.sizeRatio ?? DEFAULT_SIZE })}
            >
              {e}
            </Button>
          ))}
        </Grid>
      </FormField>
    </Stack>
  );
}
