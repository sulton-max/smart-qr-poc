import { GoogleLogin } from "@react-oauth/google";
import { GOOGLE_CLIENT_ID, signInWithGoogle } from "../api";
import type { Me } from "../types";

interface GoogleSignInButtonProps {
  onSignedIn: (me: Me) => void;
  onError?: (message: string) => void;
}

// GIS ID-token flow; renders nothing when no client id is configured (app stays guest-only).
export function GoogleSignInButton({ onSignedIn, onError }: GoogleSignInButtonProps) {
  if (!GOOGLE_CLIENT_ID) return null;

  return (
    <GoogleLogin
      onSuccess={async (cr) => {
        const idToken = cr.credential;
        if (!idToken) return;
        try {
          const me = await signInWithGoogle(idToken);
          onSignedIn(me);
        } catch (e) {
          onError?.(e instanceof Error ? e.message : "Sign-in failed");
        }
      }}
      onError={() => onError?.("Google sign-in failed")}
    />
  );
}
