import { useState } from "react";
import { Button } from "@wow-two-beta/ui/actions";
import { Card, Heading, Text } from "@wow-two-beta/ui/display";
import { Alert } from "@wow-two-beta/ui/feedback";
import { Center, Stack } from "@wow-two-beta/ui/layout";
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
    <Center className="min-h-[60vh]">
      <Card className="w-full max-w-sm p-8">
        <Stack gap="5">
          <div>
            <Heading level={1} size="xl" weight="bold">
              Welcome to Smart QR
            </Heading>
            <Text size="sm" color="muted" className="mt-1">
              Programmable codes that never expire.
            </Text>
          </div>

          {GOOGLE_CLIENT_ID ? (
            <div className="flex justify-center">
              <GoogleSignInButton onSignedIn={onAuthenticated} onError={setError} />
            </div>
          ) : (
            <Text size="xs" color="muted" align="center">
              Google sign-in isn't configured yet.
            </Text>
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

          {error && <Alert severity="danger" description={error} />}
        </Stack>
      </Card>
    </Center>
  );
}
