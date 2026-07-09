import { useEffect, useState } from "react";
import { Link, Outlet } from "react-router-dom";

import { ColorTone, SizePreset } from "@wow-two-beta/ui/foundation/utils";
import { Button, ButtonVariant } from "@wow-two-beta/ui/presentation/actions";
import { Text } from "@wow-two-beta/ui/presentation/display";
import { Spinner } from "@wow-two-beta/ui/presentation/feedback";
import { Container, ContainerSize, HStack, Navbar } from "@wow-two-beta/ui/presentation/layout";

import { UserKind, type Me } from "@/domain/identity";
import { getMe, logout } from "@/integration/identity";
import { ColorModeToggle } from "@/presentation/common";
import { GoogleSignInButton, LoginScreen } from "@/presentation/identity";
import { Logo } from "@/presentation/marketing";

/** Defines the identity-resolution phase gating what the layout renders. */
const Status = {
  /** Refers to the in-flight identity check (spinner). */
  Checking: "checking",
  /** Refers to the anonymous guest gate (login screen). */
  Gate: "gate",
  /** Refers to a resolved identity — render the routed screen. */
  Ready: "ready",
} as const;

type Status = (typeof Status)[keyof typeof Status];

// Resolves identity once: anonymous → guest gate; guests and users pass through to the routed screen.
export function AppLayout() {
  const [status, setStatus] = useState<Status>(Status.Checking);
  const [me, setMe] = useState<Me | null>(null);

  useEffect(() => {
    let cancelled = false;
    getMe()
      .then((result) => {
        if (!cancelled) {
          setMe(result);
          setStatus(result.kind === UserKind.Anonymous ? Status.Gate : Status.Ready);
        }
      })
      .catch(() => {
        // Failed identity check → treat as anonymous.
        if (!cancelled) {
          setMe(null);
          setStatus(Status.Gate);
        }
      });
    return () => {
      cancelled = true;
    };
  }, []);

  async function handleSignOut() {
    await logout();
    setMe(null);
    setStatus(Status.Gate);
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
            <ColorModeToggle />
            {status === Status.Ready && (
              <Link
                to="/app/billing"
                className="text-muted-foreground transition-colors hover:text-foreground"
              >
                Billing
              </Link>
            )}
            {me?.kind === UserKind.Guest && (
              <>
                <Text as="span" size={SizePreset.Sm} color="muted">
                  Guest
                </Text>
                <GoogleSignInButton onSignedIn={(m) => setMe(m)} />
                <Button tone={ColorTone.Neutral} variant={ButtonVariant.Outline} onClick={handleSignOut}>
                  Sign out
                </Button>
              </>
            )}
            {me?.kind === UserKind.User && me.user && (
              <>
                <Text as="span" size={SizePreset.Sm} color="muted">
                  {me.user.name}
                </Text>
                <Button tone={ColorTone.Neutral} variant={ButtonVariant.Outline} onClick={handleSignOut}>
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

      <Container as="main" size={ContainerSize.Lg} className="flex-1 px-6 py-8">
        {status === Status.Checking && (
          <div className="flex min-h-[60vh] items-center justify-center">
            <Spinner size={SizePreset.Lg} label="Loading" />
          </div>
        )}
        {status === Status.Gate && (
          <LoginScreen
            onAuthenticated={(m) => {
              setMe(m);
              setStatus("ready");
            }}
          />
        )}
        {status === Status.Ready && <Outlet />}
      </Container>
    </div>
  );
}
