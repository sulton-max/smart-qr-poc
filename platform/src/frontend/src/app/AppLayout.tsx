import { useEffect, useState } from "react";
import { Link, Outlet } from "react-router-dom";
import { Spinner } from "@wow-two-beta/ui/feedback";
import { Button } from "@wow-two-beta/ui/actions";
import { Container, HStack, Navbar } from "@wow-two-beta/ui/layout";
import { Text } from "@wow-two-beta/ui/display";
import { Logo } from "../marketing/components";
import { LoginScreen } from "../screens/LoginScreen";
import { GoogleSignInButton } from "../components/GoogleSignInButton";
import { getMe, logout } from "../api";
import type { Me } from "../types";

type Status = "checking" | "gate" | "ready";

// Resolves identity once: anonymous → guest gate; guests and users pass through to the routed screen.
export function AppLayout() {
  const [status, setStatus] = useState<Status>("checking");
  const [me, setMe] = useState<Me | null>(null);

  useEffect(() => {
    let cancelled = false;
    getMe()
      .then((result) => {
        if (!cancelled) {
          setMe(result);
          setStatus(result.kind === "Anonymous" ? "gate" : "ready");
        }
      })
      .catch(() => {
        // Failed identity check → treat as anonymous.
        if (!cancelled) {
          setMe(null);
          setStatus("gate");
        }
      });
    return () => {
      cancelled = true;
    };
  }, []);

  async function handleSignOut() {
    await logout();
    setMe(null);
    setStatus("gate");
  }

  return (
    <div className="flex min-h-screen flex-col bg-background text-foreground">
      {/* Navbar's inner Container is `px-4` vs `<main>`'s `px-6` — not overridable (see GAPS). */}
      <Navbar
        height="lg"
        sticky={false}
        bordered
        className="bg-background"
        start={
          <Link to="/" aria-label="Smart QR home">
            <Logo />
          </Link>
        }
        end={
          <HStack as="nav" align="center" gap="5" className="text-sm">
            {status === "ready" && (
              <Link
                to="/app/billing"
                className="text-muted-foreground transition-colors hover:text-foreground"
              >
                Billing
              </Link>
            )}
            {me?.kind === "Guest" && (
              <>
                <Text as="span" size="sm" color="muted">
                  Guest
                </Text>
                <GoogleSignInButton onSignedIn={(m) => setMe(m)} />
                <Button tone="neutral" variant="outline" onClick={handleSignOut}>
                  Sign out
                </Button>
              </>
            )}
            {me?.kind === "User" && me.user && (
              <>
                <Text as="span" size="sm" color="muted">
                  {me.user.name}
                </Text>
                <Button tone="neutral" variant="outline" onClick={handleSignOut}>
                  Log out
                </Button>
              </>
            )}
            <Link to="/" className="text-muted-foreground transition-colors hover:text-foreground">
              ← Back to site
            </Link>
          </HStack>
        }
      />

      <Container as="main" size="lg" className="flex-1 px-6 py-8">
        {status === "checking" && (
          <div className="flex min-h-[60vh] items-center justify-center">
            <Spinner size="lg" label="Loading" />
          </div>
        )}
        {status === "gate" && (
          <LoginScreen
            onAuthenticated={(m) => {
              setMe(m);
              setStatus("ready");
            }}
          />
        )}
        {status === "ready" && <Outlet />}
      </Container>
    </div>
  );
}
