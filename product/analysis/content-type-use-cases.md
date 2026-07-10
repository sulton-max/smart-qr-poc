# Content-Type Use Cases

*Last updated: 2026-07-07*

> Use cases per content type, framed around the wedge: every type is **dynamic + rule-routable + never-expire**, not just URLs. Feeds product framing + the SEO content-type pages (`marketing/marketing.md`, `channels/seo.md`).

## Thesis

- incumbents mostly bake **non-URL** content types as **static** (the `tel:`/`WIFI:`/vCard payload is frozen in the squares) → change = reprint.
- ForeverPin makes **every** type a dynamic code: the printed image is permanent; **who/what it resolves to follows your rules** (device · country · language · time-of-day · schedule-rota) and can be reprogrammed anytime.
- **schedule rules** are the enabler for rota/time-based routing (the on-duty-phone case) — nobody else pairs "content type" with "route by who's on shift".

---

## Resolve mechanics (two families)

- **Scheme-redirect** (`tel:` · `mailto:` · `sms:` · `geo:`) → the redirect `302`s straight to the scheme → phone opens the dialer / mail / maps. **Instant, no landing.**
- **Served-payload** (vCard · WiFi · calendar · text) → the short URL opens a tiny landing that serves the **current** payload ("Save contact" / "Connect" / "Add to calendar"). Can't `302` to a vcard, so a landing is the mechanic — and the reason these can change at all.

---

## Use cases by content type

### 📞 Phone (`tel:`) — the hero
- static limit: a printed number is frozen; the person / rota / on-call changes constantly.
- dynamic superpower: one printed code → **the right number *now*** by schedule rule (day vs night shift), location, or manual reprogram.
- use cases: **emergency contact at building entrances** (routes to who's on-duty) · "call reception" that follows the shift · lift / elevator emergency line · vending · parking-meter · gate / equipment fault line · clinic after-hours by time-of-day · site muster-point coordinator.

### ✉️ Email (`mailto:`)
- static limit: baked address; ownership moves, inbox changes.
- dynamic superpower: routes to whoever owns it now · localized inbox by `Accept-Language` · prefilled subject/body.
- use cases: "report an issue" placard → current facilities inbox · receipt feedback · warranty/claims label · localized support (EN/RU/UZ).

### 👤 vCard (contact card) — served payload
- static limit: a printed card freezes your title/number/company; a promotion = reprint the whole run.
- dynamic superpower: **never-reprint business cards** — update details anytime; role-based ("the rep on duty").
- use cases: business cards that never go stale · real-estate yard-sign agent card (agent rotates) · trade-show badge "save our contact" · team desk plaques.

### 📶 WiFi (`WIFI:`) — served payload
- static limit: SSID+password baked in; **rotating the password = reprinting every sign** (a security tax).
- dynamic superpower: printed sign stays, **rotate the password on a schedule** without reprinting; guest vs staff network by rule.
- use cases: café / hotel / Airbnb guest WiFi with monthly password rotation · office guest network · event / co-working WiFi.

### 🔗 URL — the classic
- dynamic superpower: menu-by-time · link-in-bio · app-store router (iOS/Android/desktop) · geo store-finder · A/B · campaign UTM.
- use cases: restaurant menu (lunch→dinner) · packaging → current promo · signage → localized site · one "download the app" code.

### 📍 Location (`geo:` / maps)
- dynamic superpower: update the pin as it moves.
- use cases: food-truck current spot · event wayfinding · "meet here" that changes · delivery dock / valet · pop-up location.

### 📅 Calendar event — served payload
- dynamic superpower: fix the time/venue **after** flyers print; add-to-calendar.
- use cases: printed flyers where timing shifts · recurring class schedule · venue-change resilience.

### 💬 SMS (`sms:`)
- dynamic superpower: change shortcode/number/prefilled text · route by region.
- use cases: "text to join/order/vote" campaigns · reorder-by-text on packaging · regional SMS line.

### 📝 Text
- dynamic superpower: revise the message; localize by language.
- use cases: notices/instructions that get updated · multilingual notice by `Accept-Language`.

---

## Lead with these (under-served by incumbents → SEO/marketing spears)

- **Dynamic phone** — "a QR that calls the on-duty / emergency number" (schedule-routed).
- **Dynamic WiFi** — "rotate your WiFi password without reprinting the sign".
- **Dynamic vCard** — "business cards you never reprint".
- each = its own SEO content-type page + comparison ("dynamic vCard vs static", "vs {incumbent}") — most rivals only do dynamic *URLs*.

---

## Build notes

- scheme-redirect types need the redirect service to emit `tel:`/`mailto:`/`sms:`/`geo:` from a rule result (no landing).
- served-payload types need a minimal per-type landing that renders the current payload + the native action ("Save contact" / "Connect to WiFi" / "Add to calendar").
- both reuse the same routing engine (rules → outcome); only the final emit differs.
