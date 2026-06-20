import { useState } from "react";
import { Button } from "@wow-two-beta/ui/actions";
import { Card, Heading } from "@wow-two-beta/ui/display";
import { GoogleSignInButton } from "../components/GoogleSignInButton";
import { GOOGLE_CLIENT_ID, createGuest } from "../api";
import type { Me } from "../types";

interface LoginScreenProps {
  onAuthenticated: (me: Me) => void;
}

export function LoginScreen({ onAuthenticated }: LoginScreenProps) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleContinueAsGuest() {
    setLoading(true);
    setError(null);
    try {
      const me = await createGuest();
      onAuthenticated(me);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Something went wrong");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="flex min-h-[60vh] items-center justify-center">
      <Card className="flex w-full max-w-sm flex-col gap-5 p-8">
        <div>
          <Heading level={1} className="text-2xl font-bold">
            Welcome to Smart QR
          </Heading>
          <p className="mt-1 text-sm text-muted-foreground">
            Programmable codes that never expire.
          </p>
        </div>

        {GOOGLE_CLIENT_ID ? (
          <div className="flex justify-center">
            <GoogleSignInButton onSignedIn={onAuthenticated} onError={setError} />
          </div>
        ) : (
          <p className="text-center text-xs text-muted-foreground">
            Google sign-in isn't configured yet.
          </p>
        )}

        <div className="relative">
          <div className="absolute inset-0 flex items-center">
            <div className="w-full border-t border-border" />
          </div>
          <div className="relative flex justify-center text-xs text-muted-foreground">
            <span className="bg-card px-2">or</span>
          </div>
        </div>

        <Button
          tone="primary"
          isFullWidth
          isLoading={loading}
          loadingText="Setting up…"
          onClick={handleContinueAsGuest}
        >
          Continue as guest
        </Button>

        {error && (
          <p className="rounded-md border border-destructive/40 bg-destructive/5 p-3 text-sm text-destructive">
            {error}
          </p>
        )}
      </Card>
    </div>
  );
}
