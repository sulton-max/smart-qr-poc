import type { Me } from "@/domain/identity";
import { API_BASE, problemError, readData } from "../common/client";

export async function getMe(): Promise<Me> {
  const res = await fetch(`${API_BASE}/api/identity/me`, {
    credentials: "include",
  });

  if (!res.ok) throw await problemError(res, "Identity check failed");
  return readData<Me>(res);
}

export async function createGuest(): Promise<Me> {
  const res = await fetch(`${API_BASE}/api/identity/guest`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
  });

  if (!res.ok) throw await problemError(res, "Guest creation failed");
  return readData<Me>(res);
}

// Exchanges a Google ID token for a session; backend upserts the user and sets the auth cookie.
export async function signInWithGoogle(idToken: string): Promise<Me> {
  const res = await fetch(`${API_BASE}/api/auth/google`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ idToken }),
    credentials: "include", // server sets the auth cookie on this response
  });

  if (!res.ok) throw await problemError(res, "Sign-in failed");
  return readData<Me>(res);
}

// Drops the auth cookie server-side.
export async function logout(): Promise<void> {
  const res = await fetch(`${API_BASE}/api/auth/logout`, {
    method: "POST",
    credentials: "include",
  });

  if (!res.ok) throw await problemError(res, "Logout failed");
}
