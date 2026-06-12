import { useState } from "react";
import { Button } from "@wow-two-beta/ui/actions";
import { FormField, TextInput } from "@wow-two-beta/ui/forms";
import { Card, Heading } from "@wow-two-beta/ui/display";
import { createGuest } from "../api";

interface LoginScreenProps {
  onGuest: () => void;
}

export function LoginScreen({ onGuest }: LoginScreenProps) {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleContinueAsGuest() {
    setLoading(true);
    setError(null);
    try {
      await createGuest();
      onGuest();
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

        <FormField label="Email">
          <TextInput
            value={email}
            placeholder="you@example.com"
            onChange={(e) => setEmail(e.target.value)}
          />
        </FormField>

        <FormField label="Password">
          <TextInput
            value={password}
            placeholder="••••••••"
            onChange={(e) => setPassword(e.target.value)}
          />
        </FormField>

        <Button tone="neutral" variant="outline" isFullWidth isDisabled>
          Log in
        </Button>

        <p className="text-center text-xs text-muted-foreground">
          Accounts are coming soon. No sign-up required right now.
        </p>

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
