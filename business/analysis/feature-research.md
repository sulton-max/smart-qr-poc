# ForeverPin (smart-qr) — Feature Research

> Purpose: map the QR/codes feature universe against the ForeverPin wedge (programmable-routing-first · never-expire · GWDNBM on flat pricing) to hand-pick a v0.5 batch.

*Last updated: 2026-06-24*

**Format:** one feature = one line — `**{Feature}** - {essence}. \`{fit} · {effort}\``. `fit` = core-wedge / enabler / adjacent / off-wedge / DROP · `effort` = S / M / L. Sub-bullets only for genuinely distinct sub-ideas.
**Wedge filter:** reinforces never-expire OR contextual-routing OR GWDNBM, serves the print-once SMB/dev ICP. **GWDNBM filter:** no fingerprint, no ads/nags, no engagement-bait, no scan-cap paywall levers — violators dropped.

---

## Generation form & rules (builder UX)

- **Create-form flow** - pick type → content → style → generate → download/deploy; type-tile grid entry (URL default), adaptive field set per type, live preview; tabbed setup for editing vs wizard for creating. `core-wedge · M` (type-picker + adaptive form) / `L` (full adaptive engine, follows content-templates).
- **No-code visual rule builder + JSON/API escape hatch** - visual ordered-rule editor for SMBs + canonical schema + API for devs, same engine ("Cloudflare Workers for physical codes"). `core-wedge · L`
- **Ordered first-match-wins rule set + mandatory fallback** - top-to-bottom list, first match wins, guaranteed terminal "otherwise →"; drag-reorder, per-rule enable/disable. `core-wedge · M`
- **Composable conditions (AND/OR groups)** - one rule from multiple conditions ("iOS AND in Germany AND business-hours"); "match ALL / match ANY" toggle. `core-wedge · L`
- **Adaptive mini-form per rule row** - each rule a self-contained row; pick condition → relevant operator+value inputs appear; auto-populated sub-options. `core-wedge · M`
- **Plain-language rule readout + scan simulator** - render each rule as a sentence ("If country is Japan, send to …") + "test this scan" simulator (pick fake country/device/time → see which rule wins). `core-wedge · M`
- **Dynamic destination / edit-after-print** - one stable code whose target/rules change without reprinting; edit propagates to next scan. The platform the wedge rides on (table-stakes), not the wedge itself. `enabler · S`

---

## Routing engine (smart contextual routing)

- **Guaranteed fallback** - default destination so no scan dead-ends; the fallback IS the never-brick guarantee, surfaced as an explicit promise. The foundation every other rule sits on. `core-wedge · S`
- **Device / OS routing** - resolve by OS/device family (iOS/Android/desktop, Win/macOS/Linux) → one Download code to App Store vs Play vs desktop; no prompt. `core-wedge · S`
- **Country / geo routing (IP-based)** - resolve by scanner country/region for localized pages/offers; IP-geo, no permission prompt. `core-wedge · M`
- **Language routing** - resolve by device/browser language for multilingual campaigns; no prompt, pairs with country. `core-wedge · S`
- **Time-of-day / day-of-week / scheduled-window routing** - resolve by clock, weekday, or user-chosen activation window (menu-by-daypart, event schedules); timezone (UTC vs visitor-local) is the real UX decision. `core-wedge · M`
- **A/B split / weighted routing** - split scans by percent weight (70/30, N-way), shift-all-to-winner one click; sticky per-scanner so a returning scanner keeps its bucket (never used for analytics). `core-wedge · M` (random) / `L` (sticky)
- **Random routing** - each scan → a uniformly random destination from a set (no weights); pure surprise/variety, distinct from weighted A/B split. GWDNBM-safe. `core-wedge · S`
- **Nth-scan / lottery routing** - every Nth scan (e.g. the 101st) → page Y, all others → page X; uses raw scan count only (no fingerprint → GWDNBM-safe). Limited drops / surprise reward without a scan cap. `core-wedge · M`
- **Invite-cap / limited-access routing** - first N scans → an access/invite page, all later scans → a closed/waitlist page ("first 100 users get in"); raw scan count only. NOTE: this is the CUSTOMER capping their OWN campaign (an invite mechanic), NOT the platform bricking a printed code behind a paywall — so it's wedge-safe (the rejected scan-cap-paywall is the opposite). Shares the raw-count mechanism with Nth-scan / scan-count-threshold routing. `core-wedge · M`
- **Referrer / UTM-source routing + injection** - referrer-branch routing (scan-from-IG-bio → different page) is core; UTM injection (transparent/editable params on the customer's OWN destination, no covert IDs) is an adjacent add. `core-wedge (referrer) / adjacent (UTM) · M`
- **Scan-count threshold routing** - change destination after cumulative scans cross a threshold (first N → A, rest → B); raw count only — unique-by-footprint dedup = fingerprinting, REJECT. Sits one slip from a scan cap; gate behind explicit non-cap copy. `adjacent · M` — de-scope past v1
- **Unique-vs-repeat visitor routing** - branch first-time vs returning scanner; requires persistent per-scanner identity (still tracking-adjacent). Blocked on the same guest-identity decision analytics blocks on. `adjacent · M` — de-scope until identity-semantics lands
- **Password / passcode routing branch** - interstitial asking for a password before resolving; access-control, not contextual routing. Revisit as a standalone LINK primitive. `adjacent · M` — de-scope
- **GPS radius geo-fence** - proximity routing within N km; forces a device location-permission PROMPT → violates calm/no-prompt posture. `DROP (off-wedge) · L`
- **Browser routing** - condition on scanner's web browser; niche dimension. Fold under a "device & browser" group if ever shipped. `off-wedge · S` — fold or drop

---

## Rendering & styling quality

> **Styling-emitter epic (L):** module-shapes + finder-eye + gradients all depend on one shared styling capability not yet built — they're one L epic, not three Ms.

- **Preview/export parity** - on-screen preview = byte-for-byte the download (single render source kills client/server drift); the substrate the styling epic + "it always scans" promise sit on. `core-wedge · M`
- **Module (data dot) shapes** - rounded / dots / classy / classy-rounded / extra-rounded body modules; square = max scannability, per-shape cost → pair higher ECC + quiet zone. Anchor of the styling epic. `core-wedge · L`
- **Finder-eye (corner) styling** - style the three finder patterns separately (outer frame + inner pupil, independent color); highest brand-recognition lever, but keep eye contrast ≥ body (over-stylizing = #1 designer-code failure). Sub-task of the styling epic. `core-wedge · L`
- **Foreground color & gradients** - solid colors expose in the builder now; gradients (linear/radial, color-stops, per dots/bg/corners) ride the styling epic. Contrast floor: ISO 18004 4:1 / WCAG AA 4.5:1. `core-wedge · S` (solid) / `L` (gradients, epic)
- **Center logo embedding** - drop a brand logo, engine clears modules behind it; expose logo size/margin + auto-bump ECC=H. `core-wedge · S`
- **Error-correction level control** - L/M/Q/H redundancy selector (7/15/25/30%); auto-bump Q/H on logo / heavy styling / small print. `core-wedge · S`
- **Frames + CTA captions** - outer frame + call-to-action label ("Scan for menu") with PRESERVED quiet zone; specific CTA lifts conversion (~161%), pre-set templates per use-case. Rides the styling epic. `core-wedge · M`
- **Background color / transparency** - bg color or transparent bg to sit on colored surfaces; inverted (light-on-dark) RISKY. Hard-gate behind the contrast validator so it can't brick. `adjacent · S`
- **Quiet-zone (margin) control** - configurable blank margin with a guarded non-removable floor (ISO 18004 min 4 modules); #1 cause of "works on screen, fails in print". `core-wedge · S`
- **Color/contrast scannability validator** - live guardrail scoring fg-vs-bg contrast, warns below 4:1 (ISO) / 4.5:1 (WCAG AA) before a brick; calm passive inline state, never a modal/email. `core-wedge · S`
- **Export format set (vector-first)** - SVG (default) + PNG 300+ DPI + PDF (backlog) + EPS (dev/agency); avoid JPEG (lossy → decode failures). `core-wedge · M`
- **Print-readiness / DPI guidance** - size + resolution guardrails (300 DPI, ≥3–4px/module, rule-of-10); builder shows "prints safely down to X cm". The physical-world half of never-brick; calm one-time hint. `core-wedge · S`
- **Style presets / brand templates** - one-click curated bundles (shape+eyes+gradient+frame) pre-VALIDATED to always pass contrast/ECC; serves no-designer SMB ICP, protects the "always scans" promise. Depends on the styling epic. `core-wedge · M`
- **Halftone / image-blended artistic codes** - algorithmic code resembling a target image (compute-only, no AI/GPU); styling-vanity orthogonal to never-expire + routing, endangers scannability. `off-wedge · L` — defer/drop for v1

---

## Content-type templates

> **Shared blocker:** valuable "dynamic hosted" variants (vCard page, WiFi rotate-password, menu, link-in-bio) all need a hosted-page / dynamic-content renderer the product doesn't have yet. Static payloads are cheap S; hosted variants ride shared infra = M/L.

- **URL / Website Link** - single URL field + slug + validation; the baseline routing target where smart routing attaches; UTM-injection helper. `core-wedge · S`
- **App Store smart link** - one code routes iOS→App Store / Android→Play / desktop→landing by device; a labeled preset over device-routing. `core-wedge · S`
- **Restaurant / PDF menu** - host uploaded PDF or built mobile menu page; time-of-day daypart auto-swap; edit-after-print + never-expire = #1 cafe ask. Heaviest content type — needs the hosted-page renderer; sequence LATE. `core-wedge · L`
- **vCard / digital business card** - static contact payload (near-zero infra) vs dynamic hosted contact page ("Add to contacts", editable-after-print → never-expire fit) gated on the hosted-page renderer. `core-wedge · S` (static) / `M–L` (dynamic)
- **WiFi access** - static network-credentials payload vs dynamic rotate-password-without-reprinting (cafe/gym never-expire wedge) on hosted-page infra. `core-wedge · S` (static) / `M–L` (dynamic)
- **Calendar event** - calendar-event payload or dynamic hosted variant; dynamic edits time/venue after flyers print (never-expire); timezone is the gotcha. `adjacent · M`
- **Geo / location** - map-location payload; dynamic variant routes Apple vs Google Maps by device; real-estate/venue ICP. `adjacent · S`
- **Email / SMS / Phone** - prefilled email / SMS / call payloads over the URL/redirect path. Commodity static encodes, no never-expire exercise — funnel-filler, never near the wedge. `adjacent · S`
- **Social media profile** - per-platform handle/URL (single = URL preset; multi overlaps Link-in-Bio); WhatsApp click-to-chat high-demand; device routing can deep-link native app. `adjacent · S`
- **Document / file host** - upload → hosted URL; swap-file-without-reprinting = never-expire; shares file-host backend with Menu PDF; opt-in download analytics only. `adjacent · M`
- **Link-in-bio / multi-link hub** - hosted Linktree-style multi-link page (title/bio/avatar/theme/ordered links); a hosted-page-BUILDER product smuggled in as a content type, reinforces the wedge only trivially. `adjacent · L` — de-scope to a later batch (separate product)
- **Plain text** - arbitrary text shown on scan, no network; static text cannot expire or route — no wedge mechanism applies. `off-wedge · S`
- **Crypto / pay-link** - crypto address or hosted pay-link; regulated-rails boundary risk + near-zero demand + unrelated to never-expire/routing. `DROP (off-wedge) · S`

---

## Unified code & link surface

- **1D/2D barcodes (beyond QR)** - Code128, EAN, UPC, DataMatrix, PDF417, Aztec on one unified surface; remaining work is builder-picker + export. Differentiator for "one subscription, not five". `core-wedge · S/M-low`
- **Short links + link primitives** - plain shortener + opt-in expiring / one-time / scan-capped / password-locked links (expiring/one-time = opt-in, never a paywall lever); unifies QR + barcode + short link + vCard so customers don't run five subscriptions. `adjacent · M`
- **Bulk generation (CSV in, ZIP out)** - CSV template → batch mint codes/vCards → ZIP; per-row routing/UTM injection; Pro/Agency tier-value, no wedge reinforcement — keep off the critical path. `adjacent · M`

---

## Trust, never-expire & GWDNBM posture

- **Never-expire / never-deactivate-on-downgrade (THE WEDGE)** - a lapsed/free/downgraded account keeps redirecting forever, you only lose editing/analytics; #1 one-star complaint category-wide ("ruined my 10,000 fliers"). Redirect path stays plan-agnostic; needs hardening under lapsed/free/cancelled. `core-wedge · M`
- **Unlimited scans on every tier (incl. free)** - scans never a paywall lever, no caps that brick/throttle a printed code (incumbents cap: Flowcode free 500, Linkly 500/mo); the redirect hot path stays plan-agnostic and never blocks a scan on analytics. Needs viral-burst load-test gate. `core-wedge · S`
- **GWDNBM privacy posture** - guest-ownership identity (the customer's OWN ownership marker, NOT scanner-tracking), opt-in pull analytics, cancel-export-delete, no device/IP fingerprint, no "you got a scan!" nags. Scan-count semantics: raw-per-scan only, footprint dedup = fingerprinting → settle at the analytics layer. `core-wedge · M`
- **Flat, transparent pricing** - published flat tiers from $5, cancel/export/delete anytime, no auto-renew ambush. Free $0 / Solo $5 / Pro $15 / Dev-Agency $39–49; hosted checkout + billing portal, no on-site card capture. Routing + custom domain at $5 = the commercial wedge. `core-wedge · S`
- **Guest-first creation (no signup to start)** - create + deploy a working code before any account exists, claim later; lowest-friction start, tied to never-expire (code works day 1). `core-wedge · S`
- **Trust & safety + production hardening** - keep redirects fast/safe/abuse-resistant so never-expire holds under load; split into redirect-config caching, quishing screen, rate-limit, viral-burst load-test, CDN+TLS. Abuse detection on traffic-shape, not per-scanner profiles. `adjacent · L` (bundled — split)

---

## Management, accounts & developer surface

- **Management — search, folders, tags, bulk ops** - organize many codes (search shipped; folders/tags/bulk CSV/template-locking backlog); table-stakes at scale, Pro/Agency tier. `adjacent · M`
- **Accounts, guest claim & cross-device ownership** - start anonymous, claim guest codes into a Google account, manage across devices; claim-on-sign-in shipped. Cross-device subscription merge undecided. `adjacent · M`
- **Custom domain (CNAME) on the $5 tier** - branded short domain (qr.yourbrand.com) without enterprise sales (incumbents gate at $30+); per-domain TLS + host→code resolution + CNAME verification = heaviest infra, the deliberate $5-tier wedge. Protect sequencing (after never-expire hardening + geo). `core-wedge · L`
- **Developer API + white-label / agency workspaces** - flat-priced API + keys + webhooks + per-client domains; white-label workspaces, sub-account isolation, self-serve no enterprise sales — underserved indie/agency seam. `core-wedge · L`
- **Teams / workspaces / roles (multi-user)** - multiple seats, RBAC, client workspaces, activity logs; multi-user governance orthogonal to never-expire + routing, pulls toward the rejected enterprise motion (only the agency-workspace slice is wedge-adjacent, already under the API item). `adjacent · L` — de-scope past v1

---

## Analytics (calm, opt-in pull)

- **Scan analytics — metrics & dimensions** - scans over time + breakdowns by device/OS/country/hour, opt-in pull, calm dashboard; downgrade keeps redirect, loses analytics. BLOCKED on raw-vs-unique scan-count semantics — bind to a written rule before build: RAW counts only, NO footprint dedup (DROP that sub-option), NO IP/UA at rest, aggregate-only. `adjacent · M` (data/query) / `L` (with dashboard)

---

## Dropped — GWDNBM violations / non-targets

- **Retargeting-pixel injection (Meta / Google / GA4)** - covert third-party tracking / fingerprint-adjacent; keep as an EXPLICIT non-goal ("we won't fire ad pixels on your scanners") — convert omission into positioning. `DROP`
- **Hosted landing-page / form builder** - heavier build, enterprise/marketing pull over core SMB print-once need; ForeverPin stays a routing/codes surface. `DROP for v1`
- **Enterprise governance (SSO / SOC2 / audit)** - long sales cycles conflict with flat-priced self-serve; Uniqode owns it. `DROP for v1`
- **GPS radius geo-fence · Crypto/pay-link · Plain text · Browser routing** - permission-prompt friction, regulated rails, no wedge mechanism, or pure surface growth (full notes in-category above). `off-wedge — dropped or folded`

---

## v0.5 candidates

Hand-pick shortlist for the two focus areas (Rendering + Gen-form).

| # | Feature | Area | Effort |
|---|---------|------|--------|
| 1 | Solid foreground/background color | Rendering | S |
| 2 | Center logo embedding | Rendering | S |
| 3 | Color/contrast scannability validator | Rendering | S |
| 4 | Quiet-zone floor + ECC control | Rendering | S |
| 5 | Ordered first-match rule set + mandatory fallback UI | Gen-form | M |
| 6 | Adaptive create-form (type picker + URL/App/Barcode) | Gen-form | M |
| 7 | Plain-language rule readout + scan simulator | Gen-form | M |
| 8 | Preview/export parity | Rendering | M |
| 9 | Print-readiness / DPI guidance | Rendering | S |
| 10 | Frames + CTA captions | Rendering | M |

> Sequencing: #1–4 + #9 are S quick wins. #8 (preview/export parity) unlocks #10 frames + the deferred styling epic (module-shapes / finder-eye / gradients = one L, not picked for v0.5). #5–7 are the routing-builder core. Hold the full adaptive content-template engine, A/B, composable AND/OR, and the styling epic for v0.6+.
