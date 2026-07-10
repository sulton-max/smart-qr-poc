/**
 * The REAL, scannable URLs encoded by the floating hero codes.
 *
 * These are genuine QR codes — scanning one works. Most point at the "theme of today" (a rickroll by
 * default); a couple route into the app. The gag: even the landing decorations are live codes.
 *
 * Daily update → change {@link DAILY_THEME}. Later: swap these for real ForeverPin dynamic short
 * links so the printed image never changes and only the destination rotates (the never-expire flex).
 */
export const DAILY_THEME = {
  date: "2026-07-07",
  label: "Never gonna give you up",
  url: "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
} as const;

/** Sprite payloads, weighted toward the daily theme. Kept small — one baked texture per unique URL. */
export const HERO_CODE_URLS: readonly string[] = [
  DAILY_THEME.url,
  DAILY_THEME.url,
  "https://foreverpin.com",
  "https://foreverpin.com/app/new",
];
