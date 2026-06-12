import { useEffect, useState } from "react";
import { Link, Outlet } from "react-router-dom";
import { Spinner } from "@wow-two-beta/ui/feedback";
import { Logo } from "../marketing/components";
import { LoginScreen } from "../screens/LoginScreen";
import { getMe } from "../api";

type Status = "checking" | "gate" | "ready";

/**
 * Shell for the `/app/*` routes. Resolves the visitor's identity once: anonymous visitors see the
 * guest gate (the existing `LoginScreen`), guests and registered users go straight through to the
 * routed screen. This preserves the verified guest-first flow while the marketing pages stay public.
 */
export function AppLayout() {
  const [status, setStatus] = useState<Status>("checking");

  useEffect(() => {
    let cancelled = false;
    getMe()
      .then((me) => {
        if (!cancelled) setStatus(me.kind === "Anonymous" ? "gate" : "ready");
      })
      .catch(() => {
        // A failed identity check is treated as anonymous — show the gate.
        if (!cancelled) setStatus("gate");
      });
    return () => {
      cancelled = true;
    };
  }, []);

  return (
    <div className="flex min-h-screen flex-col bg-background text-foreground">
      <header className="border-b border-border">
        <div className="mx-auto flex w-full max-w-5xl items-center justify-between px-6 py-4">
          <Link to="/" aria-label="Smart QR home">
            <Logo />
          </Link>
          <Link to="/" className="text-sm text-muted-foreground transition-colors hover:text-foreground">
            ← Back to site
          </Link>
        </div>
      </header>

      <main className="mx-auto w-full max-w-5xl flex-1 px-6 py-8">
        {status === "checking" && (
          <div className="flex min-h-[60vh] items-center justify-center">
            <Spinner size="lg" label="Loading" />
          </div>
        )}
        {status === "gate" && <LoginScreen onGuest={() => setStatus("ready")} />}
        {status === "ready" && <Outlet />}
      </main>
    </div>
  );
}
