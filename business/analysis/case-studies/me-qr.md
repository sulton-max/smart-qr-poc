# Case Study — me-qr.com

> Competitor teardown through the smart-qr (ForeverPin) lens: never-expire · contextual routing · GWDNBM calm UI. Source: live generator + feature/pricing pages, 2026-06-24.

*Last updated: 2026-06-24*

---

## What it is

- High-volume freemium QR platform (dynamic + static, 16 content templates, web + iOS/Android + API). Monetizes by **injecting a full-screen interstitial ad after every scan** on free + Lite tiers — pay to remove. Free tier already gives unlimited codes/scans + never-expire, so the ad *is* the product's tax.
- **WhatsApp generator page** (`/qr-code-generator/whatsapp`): one field — phone (with country code) + optional pre-filled message → emits a `wa.me`/`api.whatsapp.com` deep link as a (dynamic) QR. Same styling panel as every other type. Classic "one focused content-type landing page" SEO play — they have one per type (Telegram, Email, WiFi, vCard, PDF, menu, …).

---

## Shape system (the styling panel)

me-qr's styler is the **`qr-code-styling`** engine (kozakdenys) + a custom decorative shape pack. Panel exposes 6 controls: **Body shape · External Eye · Internal Eye · Single Color/Gradient · Background Color · Scannability Level**.

### Body / module shapes (~17 as flagged)

Two groups — the **engine-standard 6** (verifiable, scan-safe, what we should ship first) and an **extended decorative set** me-qr layers on (logo-silhouette / novelty — higher scan risk).

| # | Shape | Group | Distinct vs variant |
|---|---|---|---|
| 1 | Square (classic) | standard | distinct — baseline, max scannability |
| 2 | Dots | standard | distinct — circular modules |
| 3 | Rounded | standard | distinct |
| 4 | Extra-rounded | standard | **minor variant** of Rounded (radius bump) |
| 5 | Classy | standard | distinct — connected/leaf modules |
| 6 | Classy-rounded | standard | **minor variant** of Classy |
| 7 | Vertical lines / bars | extended | distinct — modules fused into vertical stripes |
| 8 | Horizontal lines / bars | extended | **mirror variant** of vertical |
| 9 | Diamond / rhombus | extended | distinct |
| 10 | Star | extended | distinct — novelty, scan-risk |
| 11 | Heart | extended | distinct — novelty, scan-risk |
| 12 | Cross / plus | extended | distinct |
| 13 | Pointed / leaf | extended | distinct |
| 14 | Fluid / blob (connected dots) | extended | distinct |
| 15 | Pixel / mosaic | extended | **variant** of Square (gapped) |
| 16 | Circle-grid | extended | **variant** of Dots |
| 17 | Rounded-square | extended | **variant** between Square & Rounded |

> Reconcile: `feature-research.md` already specs the **standard 6** under the styling epic (`Module (data dot) shapes`, line 49). The extended 7–17 are **decorative/novelty** — most are minor geometric variants, several (star/heart) are real scan-risk. **Recommendation: ship the standard 6 first; treat extended as a v-later "decorative pack" gated behind the contrast/ECC validator. Don't chase 17 for parity — half are variants, the count is a marketing number.**

### Eyes (finder patterns) — styled in two parts

- **External Eye** (outer frame ring): ~6 options — Square · Rounded · Extra-rounded · Dot/Circle · Leaf · Cut-corner. (Maps to `qr-code-styling` `cornersSquareOptions`.)
- **Internal Eye** (inner pupil): ~6 options — Square · Rounded · Dot/Circle · Diamond · Leaf · Star. (`cornersDotOptions`.)
- Outer + inner are **independently colorable** → distinct from body color.

### Color · gradient · background · scannability

- **Single Color vs Gradient** toggle. Gradient = linear **or** radial, 2 color-stops + angle; can apply to body, eyes, bg independently.
- **Background Color** — solid (incl. transparent).
- **Scannability Level** — a discrete slider/preset (roughly Low/Med/High) that trades visual density for **error-correction strength** (ECC L→H) + quiet-zone. This is me-qr's *one good guardrail* — it's the ECC knob reframed as a benefit, not a cryptic L/M/Q/H dropdown.

---

## Other good ideas

| Feature | Note |
|---|---|
| **16 content templates** | PDF · List-of-Links · Website · Apps · Coupon · Playlist · Event · Images · Business · Menu · MP3 · Feedback · WiFi · Video · vCard · (+WhatsApp/Telegram/Email/social as direct types). Each = a hosted landing page, responsive. Breadth is their moat — but most are off-wedge for us (we're routing-first, not a page-builder). |
| **Per-type landing pages** | One SEO page per content type (the WhatsApp page). Cheap, compounding organic — worth copying for our top 3–4 types. |
| **Pre-made styles** | One-click curated bundles (shape+eyes+gradient). Matches our planned `Style presets / brand templates` (feature-research line 60). |
| **"My Templates" / saved designs** | Save a styled design, reuse across codes — brand consistency without re-styling each time. Small, high-leverage for repeat SMB users. |
| **Dynamic + shaped together** | They market "even a heart-shaped code stays dynamic" — i.e. styling never costs editability. Same promise we make; good copy framing. |
| **Logo embedding** | Upload logo, auto-clears modules behind it (standard). |

---

## UX critique (chaotic vs good)

User's read confirmed: *"whole site UI looks chaotic but has good ideas."*

**Chaotic — avoid:**
- **Content-type sprawl** — 16+ template types + dozens of generator pages with inconsistent layouts; the type-picker is a wall. We stay routing-first with a tight type set.
- **Ad-after-scan** — the scanner (not even the customer) eats a full-screen interstitial. Hostile, off-brand, the antithesis of GWDNBM. **Hard avoid.**
- **Upsell density** — pricing/upgrade nags woven through the builder + dashboard; "ad-free" dangled constantly. Nag-driven, not calm.
- **Feature-per-page fragmentation** — shapes, dots, templates each live on separate marketing pages with overlapping/contradictory copy; hard to form a mental model.

**Good — borrow:**
- **Scannability Level** as a single human slider (vs raw ECC dropdown) — fits our contrast/scannability validator (feature-research lines 57, 53).
- **Independent eye styling** (outer/inner, separate color) — already our `Finder-eye (corner) styling` (line 50).
- **Per-type SEO landing pages** — cheap organic, on-wedge for our top types.
- **Saved "My Templates"** — brand-consistency lever, GWDNBM-friendly (set once, reuse).
- **"Styling never breaks dynamic"** framing — clean way to state our edit-after-print promise.

---

## Pricing / paywall posture

| Tier | Codes | Scans / lifetime | Ads after scan | Storage | Analytics retention |
|---|---|---|---|---|---|
| **Free** ($0) | 10,000 | unlimited · never-expire | **ALL codes show ads** | 100 MB | 1 yr |
| **Lite** | 100,000 | unlimited · never-expire | **1** ad-free code total (rest ad'd) | 100 MB | 3 yr |
| **Premium** (~$69/yr ≈ $5.75/mo) | 1,000,000 | unlimited · never-expire | none + no in-app ads; +email-per-scan +API | 500 MB | 3 yr |

- **No scan cap, no forced expiry on any tier** — so they *don't* monetize the brick the way Bitly/QR-Tiger do. Their lever is **ads** + analytics-retention + storage + API.
- **Paywall conflicts with our wedge** (positioning ammo):
  1. **Ad-after-scan is the free tax** → ForeverPin: *"No ads — ever. Your scanner sees your destination, not ours."* (GWDNBM core; their biggest soft underbelly.)
  2. **Ad-free is per-code, metered** (Lite = literally 1 code) → contrast with our flat, all-codes-included posture.
  3. **Analytics deleted after 1 yr on free** → minor, but pairs with our never-expire story (the code lives forever; don't quietly age out its data on the cheap tier).
  - Note: their never-expire + no-scan-cap **matches** ours — so we **don't** win on "never expires" vs me-qr. Our differentiation vs them is **(a) no ads / calm** and **(b) programmable contextual routing**, not longevity. Reserve the "never-expire" wedge for the Bitly/QR-Tiger cohort that *does* brick.

---

## Borrow / avoid / adapt for smart-qr

| Idea | Verdict | Why |
|---|---|---|
| Standard 6 module shapes | **borrow** | scan-safe core; already in styling epic |
| Extended decorative shapes (star/heart/bars/…) | **adapt** | ship later as a gated "decorative pack"; don't chase 17 for parity (half are variants) |
| Independent outer/inner eye styling | **borrow** | top brand lever; already planned |
| Single/Gradient + per-element color | **borrow** | ride the styling epic |
| **Scannability Level** slider | **borrow** | humanizes ECC; reinforces never-brick |
| Pre-made style presets | **borrow** | serves no-designer SMB ICP |
| "My Templates" (save & reuse design) | **adapt** | small, high-leverage; sequence post-accounts (v0.3+) |
| Per-content-type SEO landing pages | **adapt** | do for our top 3–4 types only, consistent layout |
| 16 hosted page-builder templates | **avoid** | off-wedge — we route, we're not a page-builder |
| **Ad-after-scan monetization** | **avoid** | violates GWDNBM; it's our wedge *against* them |
| Per-code metered ad-free + upsell nags | **avoid** | calm flat pricing instead |
| Feature-fragmented marketing UX | **avoid** | one coherent builder + mental model |
