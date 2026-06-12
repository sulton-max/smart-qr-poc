import { Link, NavLink, Outlet } from "react-router-dom";
import { Button } from "@wow-two-beta/ui/actions";
import { Logo } from "./components";
import { MarketingFooter } from "./MarketingFooter";

function navLinkClass({ isActive }: { isActive: boolean }) {
  return `text-sm transition-colors ${
    isActive ? "font-medium text-foreground" : "text-muted-foreground hover:text-foreground"
  }`;
}

/** Public marketing shell — sticky header with nav + CTAs, then the routed page, then the footer. */
export function MarketingLayout() {
  return (
    <div className="flex min-h-screen flex-col bg-background text-foreground">
      <header className="sticky top-0 z-40 border-b border-border bg-background/80 backdrop-blur">
        <div className="mx-auto flex max-w-6xl items-center justify-between gap-4 px-6 py-3.5">
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
          <div className="flex items-center gap-2">
            <Button asChild variant="ghost" tone="neutral" size="sm" className="hidden sm:inline-flex">
              <Link to="/app">Open app</Link>
            </Button>
            <Button asChild tone="primary" size="sm">
              <Link to="/app/new">Get started</Link>
            </Button>
          </div>
        </div>
      </header>

      <main className="flex-1">
        <Outlet />
      </main>

      <MarketingFooter />
    </div>
  );
}
