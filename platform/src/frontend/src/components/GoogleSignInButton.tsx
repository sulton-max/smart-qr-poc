import { GoogleLogin } from "@react-oauth/google";
import { GOOGLE_CLIENT_ID, signInWithGoogle } from "../api";
import type { Me } from "../types";

interface GoogleSignInButtonProps {
  /** Called with the resolved account after a successful sign-in (and guest-code claim). */
  onSignedIn: (me: Me) => void;
  /** Called with a message when sign-in fails. */
  onError?: (message: string) => void;
}

/**
 * Google sign-in button (GIS ID-token flow). Renders Google's button, exchanges the returned credential for a
 * session via the backend, and reports the resolved account. Renders nothing when no client id is configured
 * (`VITE_GOOGLE_CLIENT_ID` unset) — the app stays guest-only.
 */
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
