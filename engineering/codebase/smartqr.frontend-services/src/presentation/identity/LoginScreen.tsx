import { useState } from "react";

import { ColorTone, SizePreset } from "@wow-two-beta/ui/foundation/utils";
import { Button } from "@wow-two-beta/ui/presentation/actions";
import { Card, Heading, Text } from "@wow-two-beta/ui/presentation/display";
import { Alert } from "@wow-two-beta/ui/presentation/feedback";
import { Center, Divider, Stack } from "@wow-two-beta/ui/presentation/layout";

import type { Me } from "@/domain/identity";
import { GOOGLE_CLIENT_ID } from "@/integration/common";
import { createGuest } from "@/integration/identity";
import { GoogleSignInButton } from "./components/GoogleSignInButton";

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
      <Card className="surface-soft w-full max-w-sm p-8">
        <Stack gap="5">
          <div>
            <Heading level={1} size={SizePreset.Xl} weight="bold">
              Welcome to Smart QR
            </Heading>
            <Text size={SizePreset.Sm} color="muted" className="mt-1">
              Programmable codes that never expire.
            </Text>
          </div>

          {GOOGLE_CLIENT_ID ? (
            <div className="flex justify-center">
              <GoogleSignInButton onSignedIn={onAuthenticated} onError={setError} />
            </div>
          ) : (
            <Text size={SizePreset.Xs} color="muted" align="center">
              Google sign-in isn't configured yet.
            </Text>
          )}

          <Divider label="or" />

          <Button
            tone={ColorTone.Primary}
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
