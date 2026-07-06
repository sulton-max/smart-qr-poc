import { Button } from "@wow-two-beta/ui/presentation/actions";
import { Moon, Sun } from "lucide-react";
import { useColorMode } from "@wow-two-beta/ui/foundation/primitives";

/** Top-bar light/dark switch. Uses the local color-mode hook (→ SDK ColorModeProvider after the bump). */
export function ColorModeToggle() {
  const { mode, toggle } = useColorMode();
  return (
    <Button
      variant="ghost"
      tone="neutral"
      shape="square"
      aria-label={mode === "dark" ? "Switch to light mode" : "Switch to dark mode"}
      onClick={toggle}
    >
      {mode === "dark" ? <Sun size={18} /> : <Moon size={18} />}
    </Button>
  );
}
