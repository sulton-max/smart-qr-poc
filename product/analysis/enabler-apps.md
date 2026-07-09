# ForeverPin (smart-qr) — Enabler-App Ideas

*Last updated: 2026-07-07*

> Apps that sit **ON** ForeverPin as the enabler — the never-expire code + programmable routing that points at them. ForeverPin's *own* feature universe → [`feature-research.md`](feature-research.md); this doc is the layer above: products the stack **unlocks**, not features of the core.
> A save-for-later ideas doc, not a spec. Working codenames (`Pin*`) are placeholders.

## The enabler stack (what every app here composes)

ForeverPin today is a **router**, not a destination — it points a scan somewhere. Each app below adds a **hosted destination** and turns ForeverPin from "smart redirect" into "smart redirect **+** the page it lands on." Three primitives:

| # | Primitive | Status | The emotional hook |
|---|---|---|---|
| **P1** | **Never-expire code** — print/stick once, never bricks on downgrade | shipped (the wedge) | a dead code = reprinted flyers, a lost pet, a "sold" sign to nowhere |
| **P2** | **Programmable routing** — one code → many destinations by context (time · geo · device · status · sentiment) | shipped | "the sign updates itself" |
| **P3** | **Hosted destination page** — a ForeverPin-hosted mini-page the code lands on (hub · card · menu · contact · listing) | **NEW — the enabler-app surface** | the thing you don't have to build or host elsewhere |

> Strategic read: the free styled-QR generator is top-of-funnel; **dynamic routing is the paid wedge; these apps are how P3 gets built** — each ships one hosted page-type and pulls ForeverPin from redirect-tool toward destination-platform. Tag per feature: `v1` (MVP) · `later`.

---

## PinHub — the never-expire link hub  *(seed #1)*

**Pitch:** one hosted page holding all your links + contact info, behind a QR/short-link that never dies. Linktree, minus the hostage codes and the 12% cut.

**Problem:** link-in-bio pages are the default "one link" surface, but incumbents (a) can lock the page or brand it on downgrade, (b) skim commerce fees, and (c) are **static** — the same page for everyone, everywhere.

**Enabler role:** `P3` hosts the hub; `P1` guarantees the printed/shared code outlives any plan change; `P2` makes the hub **contextual** — the one differentiator none of the flat-page incumbents have (show different links by country / device / time from a single code).

| Feature | When | Note |
|---|---|---|
| Hosted hub page — links, avatar, socials, vCard button | `v1` | the P3 MVP; theme presets |
| QR + short link to the hub, **never-expire** | `v1` | rides P1 — the anti-Linktree headline |
| Contextual blocks — link visible by geo/device/time | `v1` | `moat` — rides P2; "tour dates only in-country", "App Store on iOS" |
| Click analytics (calm, no nags) | `v1` | GWDNBM: no "you got a click!" email |
| Custom domain · logo removal | `later` | table-stakes parity, cheap tier |
| Lead capture (email/SMS), embeds, product links | `later` | only if it stays GWDNBM (no dark patterns) |

**Who:** creators, freelancers, SMBs, event booths, anyone printing "scan me" on a card/flyer/sign.

| Competitor | Their shape | ForeverPin edge |
|---|---|---|
| **Linktree** | Free→$35; unlimited links; **12%→0% commerce fee** by tier; price hike Nov 2025 | never-expire code, flat price, **routed** hub, no commerce cut |
| **Beacons.ai** | monetization suite — store, media kit, invoicing, AI setup | we're lighter/calmer; routing not a store |
| **Bio.link** | minimalist, free, ad-free | parity on calm + a routing layer they lack |

---

## PinCard — printable digital business card  *(seed #2)*

**Pitch:** design a card in-app, print it (or save to wallet), and its QR opens a live contact page that you update forever without reprinting. Composes **PinHub** + a card template.

**Problem:** paper cards go stale the day your title/number changes; the NFC-card players (Popl, Blinq, HiHello) solve it but **sell you hardware** and lock the profile behind a subscription — change vendors and the card is a brick.

**Enabler role:** `P1` is the hero — the *printed* QR never expires, so "new job, same card, updated details" just works with no hardware. `P3` hosts the contact/vCard page (a PinHub variant); `P2` optional (route by language/region for international networking).

| Feature | When | Note |
|---|---|---|
| Card template gallery — printable front/back designs | `v1` | the visible deliverable; export print-ready PDF/SVG |
| QR → hosted vCard page (tap-to-save contact) | `v1` | rides PinHub's P3 renderer |
| Edit details anytime, code unchanged | `v1` | `moat` — rides P1; the never-reprint promise |
| Apple/Google Wallet pass | `later` | parity with Blinq/HiHello |
| Two-way contact exchange / lead capture | `later` | Popl's paid hook; keep GWDNBM |
| NFC option (optional sticker, not required) | `later` | QR-first; NFC is add-on, never the lock-in |

**Who:** sales/field reps, founders, conference-goers, trades handing out cards door-to-door.

| Competitor | Their shape | ForeverPin edge |
|---|---|---|
| **Popl** | NFC-first, $0/$8/$15 + $25–50 cards; team GTM/lead-capture | **no hardware lock-in**; printed QR never bricks; flat price |
| **Blinq** | $2.99/mo, AI contact enrichment, Wallet, SSO | print-native + never-expire; we don't need their card |
| **HiHello** | QR/NFC/Apple Watch, video, SOC 2 | lighter, cheaper, code-never-expires framing |

---

## PinMenu — menu-by-daypart for venues  *(mine)*

**Pitch:** one laminated table code that shows breakfast at 8am, lunch at noon, cocktails at 7pm — and never needs re-laminating when the menu changes.

**Problem:** restaurants either reprint menus constantly or run clunky QR-menu suites priced for ordering/POS they don't want. And the daypart problem (same code, different menu by time) is exactly what static QR menus can't do.

**Enabler role:** `P2` time-of-day routing is the headline (already a shipped core-wedge route — "menu-by-daypart"); `P1` means print/laminate once; `P3` hosts the menu page itself. This is the **most on-ICP** idea — product.md names "restaurants (menu-by-time)" as the primary user.

| Feature | When | Note |
|---|---|---|
| Hosted menu page — sections, items, prices, photos | `v1` | P3 MVP; PDF/image → menu import |
| Daypart routing — one code → breakfast/lunch/dinner | `v1` | `moat` — rides P2 time routing |
| Real-time price/86'd-item edits, code unchanged | `v1` | rides P1; no reprint |
| Multi-language by device locale | `later` | rides P2 language routing; tourist venues |
| Allergen/nutrition tags, seasonal specials | `later` | display only |
| Ordering / payments | `drop` | **off-wedge** (product.md: not payments-rail); stay a display+routing layer |

**Who:** cafés, bars, food trucks, hotels, multi-daypart restaurants.

| Competitor | Their shape | ForeverPin edge |
|---|---|---|
| **MenuTiger / QR Tiger** | AI menu maker, ordering, Stripe, analytics — full POS-ish suite | we're **display + daypart routing**, flat + never-expire; not trying to be a POS |
| **GloriaFood / Flipdish** | ordering-led | same — we don't compete on ordering, we win on print-once + context |

---

## PinReview — sentiment-routed feedback gateway  *(mine)*

**Pitch:** a table/receipt code that sends delighted customers to leave a public review and unhappy ones to a private "tell us first" form — one code, routed by how they tap.

**Problem:** SMBs want more 5-star reviews and fewer public 1-stars, but bolting this onto reputation suites (Birdeye, NiceJob) is expensive, and naive "QR straight to Google" flows are increasingly **filtered** in 2026.

**Enabler role:** `P2` routing on a self-reported sentiment tap (👍 → public review invite · 👎 → private feedback) — a new routing *condition*, not a tracking one, so it's GWDNBM-safe; `P1` for the durable table sticker; `P3` hosts the private-feedback form.

| Feature | When | Note |
|---|---|---|
| Sentiment splash → route happy vs unhappy | `v1` | `moat` — rides P2; the whole idea |
| Hosted private-feedback form (unhappy path) | `v1` | P3; owner gets it before it's public |
| Deep-link to Google/Yelp/TripAdvisor (happy path) | `v1` | configurable destination |
| Never-expire sticker/standee | `v1` | rides P1 |
| Response analytics, per-location codes | `later` | multi-site SMBs |
| **Ethics/risk gate** | note | don't *suppress* reviews — invite public, capture private; heed 2026 review-filtering + gating scrutiny. Frame as feedback triage, not review-gating |

**Who:** restaurants, salons, clinics, home-services, any receipt/counter surface.

| Competitor | Their shape | ForeverPin edge |
|---|---|---|
| **Birdeye** | enterprise reputation suite, review funnel landing pages, $$$ | cheap, single-purpose, never-expire, flat |
| **NiceJob** | affordable Google-review QR | we add the **sentiment split** + private capture in the routing layer |

---

## PinBack — return-to-owner tags  *(mine)*

**Pitch:** a QR sticker for keys, luggage, laptops, water bottles, pet collars — scan it and reach the owner, who set it up once and never has to touch it again.

**Problem:** the whole value of a lost-and-found tag is that it *works when it matters* — years after you stuck it on. A tag whose code expires (or whose vendor folds) is worse than no tag. This is the single most **never-expire-critical** use case there is.

**Enabler role:** `P1` is life-or-death here (a pet tag that bricks is catastrophic) — the strongest possible expression of the wedge; `P3` hosts the "you found my stuff" contact page (reveal-my-number, offer a reward, message-to-owner); `P2` lets the owner re-route (update contact, toggle "lost mode") with no new sticker.

| Feature | When | Note |
|---|---|---|
| Hosted found-item page — masked contact, message-to-owner | `v1` | P3; privacy-first (no raw phone/address exposed) |
| Never-expire tag code | `v1` | `moat` — rides P1; the entire promise |
| Scan alert to owner (email/SMS) + coarse scan location | `v1` | opt-in; GWDNBM (finder consents, no covert tracking) |
| "Lost mode" toggle — swap page to reward/urgent | `later` | rides P2 status routing |
| Printable sticker sheets / order pre-printed tags | `later` | pet-collar, luggage, gear formats |
| One dashboard across all your tags | `later` | ties into the ForeverPin account |

**Who:** pet owners, travelers, gear-heavy hobbyists (cyclists, photographers), parents labeling kids' stuff, offices tagging equipment.

| Competitor | Their shape | ForeverPin edge |
|---|---|---|
| **Dynotag** | free QR → owner page, scan alerts + GPS map, "maintenance-free lifetime" | same core, but **one account across tags + your other ForeverPin codes**; routing to re-target |
| **PetHub** | pet-specific profile + QR tag, licensing tie-ins | generic (any item), flat price, re-routable |
| **Tile / AirTag** | BLE/UWB hardware trackers | different tech; QR needs no battery/hardware, never-expire, cents to print |

---

## PinSign — time/status-routed signage & event pages  *(mine)*

**Pitch:** a physical sign whose destination changes by **date or status** — a "for sale" sign that becomes "sold → my next listing," an event sign that flips from *info* to *live schedule* to *photo gallery* as the day arrives.

**Problem:** signage and event collateral are printed weeks ahead but need to mean different things over time. Real-estate agents literally want "when it sells, change the URL — the printed code stays the same"; event organizers want one code that evolves pre/during/post.

**Enabler role:** `P2` time/status routing is the engine (already shipped: scheduled-window + status routes); `P1` lets the agent **reuse the same physical sign** across listings and the couple print invites months early; `P3` hosts the listing/event page.

| Feature | When | Note |
|---|---|---|
| Hosted page — listing / event details | `v1` | P3; template per vertical (property, wedding, conference) |
| Status routing — for-sale → under-offer → sold → next | `v1` | `moat` — rides P2; the reusable-sign play |
| Scheduled phases — info → schedule → gallery by date | `v1` | rides P2 scheduled-window routing |
| Never-expire sign/invite code | `v1` | rides P1; print months ahead, reuse signs |
| Lead/RSVP capture, per-sign analytics | `later` | agent lead-gen, event headcount |
| Bulk codes for multi-listing / multi-sign campaigns | `later` | agency/organizer tier |

**Who:** real-estate agents, event organizers, weddings, conferences, pop-ups, open houses, yard-sale/estate-sale sellers.

| Competitor | Their shape | ForeverPin edge |
|---|---|---|
| **OwnQRCode / Flowcode / QR Tiger (RE)** | dynamic code → property page; "lifetime, no monthly fees" claims | generic status+phase routing + hosted page, one flat account, genuinely never-expire |
| **Etsy sign templates** | static printable QR signs | ours re-routes; a static sign can't flip to "sold" |

---

## Cross-cutting

- **Shared substrate:** every app reuses the same P3 page renderer + P1 never-expire redirect + calm analytics. Build the hosted-page renderer once (PinHub), and PinCard/PinMenu/PinReview/PinBack/PinSign are mostly *templates + one routing condition* on top.
- **Funnel:** the free styled-QR generator feeds all of them — "made a code, now give it a page" is the natural upsell into any app.
- **GWDNBM guardrail:** these apps must not smuggle in the dark patterns the wedge rejects — no scan-cap paywalls, no "you got a scan!" nags, no covert finder-tracking (PinBack), no review-suppression (PinReview).

## Sequencing — the natural first expansion

**PinHub first.** Rationale:

1. **It builds P3** — the hosted-destination-page renderer that PinCard, PinMenu, PinReview, PinBack, and PinSign *all* reuse. Everything else is templates + one routing condition on that renderer, so PinHub is the unlock, not just an app.
2. **PinCard is nearly free right after** — it's PinHub + a print template; the two seed ideas compose (a printed card whose QR opens a hub), giving a fast second launch off shared code.
3. **Widest audience, lowest lift** — link hubs need no vertical knowledge (unlike menus/real-estate) and land the same "never-expire + flat price" pitch the core already markets.
4. **On-wedge without scope creep** — pure display + routing, no payments/ordering/hardware; it strengthens P3 without dragging ForeverPin off the redirect-platform thesis.

Then order by ICP overlap with the current core: **PinMenu** (product.md's stated primary user, and daypart routing already ships) → **PinSign** (reuses shipped status/scheduled routing; strong RE pull) → **PinBack** (purest never-expire story, but needs privacy/alert plumbing) → **PinReview** (smallest build, but gate on the review-ethics/2026-filtering risk first).
