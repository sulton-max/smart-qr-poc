# ForeverPin — Marketing Handoff

*Last updated: 2026-07-07*

> Continuation point for the marketing thread (branding · domain · GTM conventions · promo · landing hero sim). Self-contained — read this first, then the linked docs.

## Status

- Product **#002**, pre-launch, on a polish track. **Name + domain locked; marketing playbooks written; landing hero sim in progress.** Code still says "Smart QR" until the rebrand lands.

---

## Locked decisions

- **Name:** ForeverPin · **Domain:** `foreverpin.com` (bought at **Cloudflare**, $10.46 flat, at-cost).
- **Tagline:** "Pin it once. It points forever." (pin = print-once permanence · points = smart routing).
- **Positioning v2:** *free styled QR generator (magnet) + paid dynamic forwarder (wedge)*. One-liner: "Make any QR, free. Pay only when you want it to forward — and never expire."
- **Pricing experiment:** free = unlimited static generation (any style); paid entry **$1/mo but billed annually ($12/yr)** to dodge Stripe's ~33% fee on $1 monthly; keep a Pro/API tier for ARPU. Open: free *dynamic* allowance (1? 3?) · test $1-annual vs $3.
- **Runners-up (logged):** `lodestar.io` · `routestone.io` · `permacode`.

---

## Conventions written (wow-two — reusable across apps)

`wow-two-ws/conventions/marketing/` (index: `marketing-conventions.md`):
- `brand-naming-and-domains.md` — style taxonomy · scoring rubric · verification runbook · domain strategy.
- `go-to-market.md` — laws (distribution≈70% · SEO+short-form primary · retention-first · fee-efficient pricing) · launch sequence · metrics · checklist.
- `channels/` — `channels.md` (catalog + audience-fit; maker platforms ≠ ICP) · `seo.md` · `content-formats.md` (17 formats) · `meme-templates.md` (~24 named meme/audio shells: Nobody's-gonna-know, two-button, expanding-brain, POV…).

---

## App docs + assets built

- `product/marketing/marketing.md` — app GTM (positioning v2 · $1 experiment · channels).
- `product/marketing/landing-hero-concept.md` — the hero-sim concept + v2 mode ideas.
- **Marketing site (live):** landing + pricing + blog + **4 SEO seed posts** (`.../frontend-services/src/presentation/marketing/`).
- **Promo assets:** `ventures/smart-qr-promo/` — Remotion hero video/GIF (light+dark) + feature/i18n-routing stills. ⚠ still "Smart QR"-branded.

---

## Interactive hero sim (v0.9 — experiment track)

- code: `.../frontend-services/src/presentation/marketing/hero/` → `heroCodes.ts` · `HeroCanvas.tsx` · `HeroSim.tsx`; wired in `LandingPage.tsx`. Log: `engineering/planning/version-track/v0.9/v0.9.md`.
- **done:** real scannable QR "card" sprites (daily-theme URL, rickroll default → `heroCodes.ts` `DAILY_THEME`). Modes **drift** (cruise + wall-bounce · click pushes codes away) · **bump** (weight-based ramming · random-waypoint roam · ⅓ randomly rest 2-3s · click = 1s truce) · **chase** (gather around cursor w/ dead-zone). Clash-detection always on. Dev control bar (mode + live mode-aware sliders). Perf: DPR 1.5 · 60fps cap · IntersectionObserver + hidden-tab pause · count auto-reduce on narrow viewports.
- **to finish:** dial modes with the sliders → **bake liked values as `DEFAULT_PARAMS` + remove the dev bar** → random-mode-per-refresh · v2 modes (swarm/route/assemble) + scannable Easter-egg codes + swap to real ForeverPin **dynamic** short links (image fixed, destination rotates daily) · deeper **mobile** responsive pass.
- verify in a real browser (`localhost:7025`) — headless preview can't free-run it (hidden tab suspends rAF = the perf pause).

---

## Open / next (ordered)

1. **Grab handles** — `@foreverpinapp` on X + Instagram (bare `foreverpin` taken on IG); `foreverpin` GitHub org (optional). Kit in `marketing.md` open-questions.
2. **Full rebrand** `Smart QR → ForeverPin` (code strings + promo). Tracked → `engineering/planning/version-track/` (was `Brand & rebrand` backlog) — folder/namespace renames later.
3. **Finish the hero sim** (bake defaults · remove dev bar · v2 modes · mobile).
4. **Free styled-generator magnet page** — the front door + Product-Hunt/SEO hook.
5. **Trademark clearance** on "ForeverPin" (USPTO/EUIPO) before hard launch.
6. **Content + launch:** SEO content-type pages (`/vcard` · `/wifi` · `/dynamic-qr` · `/qr-that-never-expires`) + comparison pages ("QR Tiger/Bitly alternative", "vs static") · short-form **10-pack scripts** (map wedges → meme templates) · IndieHackers + Reddit → Product Hunt · **Canva app** · wedding/event niche (Pinterest/Etsy).
7. **Pricing/analytics:** decide free dynamic allowance · $1-annual vs $3 test · privacy-first (Plausible) funnel.

---

## Offered but not yet done

- the short-form **10-pack scripts** · a this-week **trending-sounds shoot-list** (ephemeral → keep out of the convention).

---

## Run

- `pnpm -C workbench/ventures/smart-qr-poc/engineering/codebase/smartqr.frontend-services dev` → `https://localhost:7024` (or the preview `:7025`). Backend (for `/app`) needs Postgres + the API.
