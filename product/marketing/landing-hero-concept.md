# ForeverPin — Landing Hero Concept

*Last updated: 2026-07-07*

> An interactive **QR-simulation background** for the landing hero — floating/reacting codes, a random mode per refresh — replacing the static header. Analysis + build spec. General pattern ("interactive on-brand hero") is a candidate convention later.

## The idea (yours)

- background = a **simulation of floating QR codes**; on refresh, load 1 of ~3-5 **modes** (drift · bump/"fight" · …).
- goal: a fun, memorable landing that earns attention + shares — the site itself becomes content.

## Why it works

- **on-brand** — the product *is* QR codes; the hero demonstrates the vibe instead of describing it.
- **shareable** — "the QR site where they fight" = organic WOM; screen-record it → a short-form clip (closes the loop with `content-formats.md`).
- **dwell + replay** — interactive keeps people on-page; random mode gives a reason to refresh/share.

---

## Killer upgrade — the codes are REAL

- make the floating codes **actual scannable ForeverPin codes** → scanning one opens an Easter egg (a coupon · a "you found it" page · the builder).
- the flex: *even the decorations are live, dynamic, never-expire codes* — the hero **is** the product. This is the detail that makes it spread.

---

## Modes (tie each to a product story, not random)

| Mode | Motion | Story it tells |
|---|---|---|
| **Drift** (default, calm) | gentle zero-g float | the GWDNBM calm baseline |
| **Swarm** (boids) | school like fish, then split toward targets | "one code → many destinations" (routing) |
| **Bump** (physics) | collide + jostle; brief color-swap on hit | playful "fight" + shows styling |
| **Route** | travel paths, sort into buckets (iOS · DE · lunch) | **literally visualizes smart routing** — most on-message |
| **Assemble** | scattered modules snap into one big hero QR (scannable → app) | "print once" reveal + a logo moment |
| **Immortal** (Easter) | codes try to fade/glitch → snap back / respawn | "codes that never die" gag |

---

## Interactions

- **cursor** — codes repel/attract/scatter from the pointer (adds to any mode).
- **click a code** → it flies to center + opens the create flow (playful CTA).
- **shuffle button** — reroll the mode without a full refresh; tiny label ("mode: swarm") invites "what else is there?".
- **hidden mode** — a Konami-triggered QR pong/breakout for sharers.

---

## Guardrails (non-negotiable)

- **perf** — canvas 2D (≤~60 sprites) or lightweight WebGL (Pixi/OGL); **pre-render each QR to a texture once**, reuse; cap count, **reduce on mobile**, pause on hidden tab → 60fps, no battery drain.
- **a11y** — `prefers-reduced-motion` → a calm static hero (respects the brand + accessibility).
- **legibility** — a scrim/vignette behind the headline; the **pitch + CTA stay foreground** and never get buried by the fun.
- **tone** — playful, not chaotic; keep the trustworthy never-expire promise intact.
- **SEO** — real `H1`/pitch stay in the DOM (animation is a background layer only).

---

## Tech sketch

- a `<HeroCanvas>` layer behind the existing hero content in `LandingPage.tsx`; mode chosen on mount (random 1-of-N), gated by reduced-motion.
- physics: tiny hand-rolled or `matter-js` (bump) · boids (swarm); QR textures generated once via the existing `qrcode.react`/offscreen canvas.

---

## Rollout (don't block launch)

- **v1** — Drift + Bump (2 modes) + reduced-motion fallback + scrim. A delightful touch, ship small.
- **v2** — Route + Assemble + real-scannable Easter-egg codes (the shareable spike).
- it's polish, not a launch gate — but the **Route/Assemble** modes are the ones worth the effort (they sell the product).
