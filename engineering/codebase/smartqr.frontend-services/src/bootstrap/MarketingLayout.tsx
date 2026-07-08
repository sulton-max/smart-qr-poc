import { Link, NavLink, Outlet } from "react-router-dom";

import { ColorTone, SizePreset } from "@wow-two-beta/ui/foundation/utils";
import { Button, ButtonVariant } from "@wow-two-beta/ui/presentation/actions";
import { Container, ContainerSize, HStack } from "@wow-two-beta/ui/presentation/layout";

import { ColorModeToggle } from "@/presentation/common";
import { Logo, MarketingFooter } from "@/presentation/marketing";

function navLinkClass({ isActive }: { isActive: boolean }) {
  return `text-sm transition-colors ${
    isActive ? "font-medium text-foreground" : "text-muted-foreground hover:text-foreground"
  }`;
}

/** Public marketing shell — header, routed page, footer. */
export function MarketingLayout() {
  return (
    <div className="flex min-h-screen flex-col bg-background text-foreground">
      <header className="sticky top-0 z-40 border-b border-border bg-background/80 backdrop-blur">
        <Container
          size={ContainerSize.Full}
          className="flex max-w-6xl items-center justify-between gap-4 px-6 py-3.5"
        >
          <Link to="/" aria-label="Smart QR home">
            <Logo />
          </Link>
          <nav className="hidden items-center gap-7 sm:flex">
            <NavLink to="/pricing" className={navLinkClass}>
              Pricing
            </NavLink>
            <NavLink to="/blog" className={navLinkClass}>
              Blog
            </NavLink>
          </nav>
          <HStack align="center" gap="2">
            <ColorModeToggle />
            <Button asChild variant={ButtonVariant.Ghost} tone={ColorTone.Neutral} size={SizePreset.Sm} className="hidden sm:inline-flex">
              <Link to="/app">Open app</Link>
            </Button>
            <Button asChild tone={ColorTone.Primary} size={SizePreset.Sm}>
              <Link to="/app/new">Get started</Link>
            </Button>
          </HStack>
        </Container>
      </header>

      <main className="flex-1">
        <Outlet />
      </main>

      <MarketingFooter />
    </div>
  );
}
