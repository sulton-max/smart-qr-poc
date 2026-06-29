# v0.5 · Iteration — Code styling

*Last updated: 2026-06-24*

> Transient iteration spec (per `version-track.md` § Iteration spec docs). Review + refine BEFORE implementing; delete when the iteration is done. Plan only — what + how, no logs.

## Goal

One server-authoritative render path (matrix + `StyleSpec` → SVG) that every styling feature plugs into without re-architecting, and a preview that is the same bytes as the export.

## Why now (substrate)

- v0.5 styling epic (shapes · finder-eye · gradients · frames · presets) all ride **one** emitter — build that emitter first or every feature re-touches rendering.
- Current drift is live: preview = `qrcode.react` (client, user colors) vs export = QRCoder server (hardcoded `#000`/`#FFF`, `StyleJson` unused). Two engines → guaranteed mismatch. Fix = single source.

## Current state (what we're replacing)

- **Matrix:** QRCoder `QRCodeGenerator.CreateQrCode` (QR) · ZXing.Net (barcodes). Keep both as matrix sources.
- **SVG:** QRCoder `SvgQRCode.GetGraphic` — one `<rect>` per dark module, **square-only**, no shape/finder/gradient seam. ← the wall.
- **PNG:** QRCoder `PngByteQRCode` → ImageSharp center-overlay (PNG-only, simple overlay, no module knockout).
- **Options:** `CodeRenderOptions` = fg/bg hex · pixels-per-module · ECC · optional logo PNG. `CodeImageService` never passes them (defaults only).
- **Preview:** `frontend/src/components/QrPreview.tsx` → `QRCodeSVG` (qrcode.react). Separate code path.
- **Anchor:** `CodeEntity.StyleJson` JSONB already exists (empty `"{}"`) — the `StyleSpec` home, no migration needed.

## Must-fit checklist (architecture has to absorb all of these as fields/passes, not rewrites)

- [ ] Module shapes — square / rounded / dots / classy
- [ ] Finder-eye styling — outer frame + inner pupil, independent (color/shape)
- [ ] Foreground color + gradients — linear/radial, color-stops, per dots/bg/corners
- [ ] Background color + transparency
- [ ] Center logo — clear (knock out) modules behind it, force ECC≥H
- [ ] Frames + CTA captions — outer frame + label, quiet zone preserved
- [ ] Quiet-zone floor — guarded min 4 modules (ISO 18004)
- [ ] Error-correction level — L/M/Q/H, auto-bump on logo/heavy style
- [ ] Export — SVG (vector) + PNG (300+ DPI); PDF/EPS later (off the same SVG)
- [ ] 1D/2D barcodes — Code128 · EAN · UPC · DataMatrix · PDF417 · Aztec on the unified surface
- [ ] Style presets — pre-validated bundles (shape+eyes+gradient+frame)
- [ ] Preview = export parity — single render source, byte-for-byte

## Recommended architecture

### 1. Emitter — custom framework-agnostic SVG path emitter (DECISION)

- `(BitMatrix + StyleSpec) → SVG string`: per-module `<rect>`/`<circle>`/rounded `<path>`, finder patterns as their own groups, `<linearGradient>`/`<radialGradient>` defs, `<image>` logo with module knockout, frame + caption group, computed quiet zone.
- Matrix from QRCoder/ZXing (unchanged) → emitter consumes the bit grid only; `SvgQRCode`/`PngByteQRCode` are retired as the styled path (kept only as a fast plain fallback if useful).
- **Why custom:** QRCoder = no shape/finder/gradient control; ZXing SVG = plain only.
- _Alt — QRCoder ArtQRCode:_ needs System.Drawing → broken on Linux/Docker. Reject.
- _Alt — wrap a JS styler (qr-code-styling) server-side via node:_ extra runtime + lang boundary. Reject.

### 2. Rasterization — SVG → PNG (and later PDF/EPS), NO System.Drawing

- **Pick: Svg.Skia (SkiaSharp under it) (DECISION)** — render our emitter's SVG to PNG/PDF; SkiaSharp ships native libs (Linux/Docker via `SkiaSharp.NativeAssets.Linux`, add to the Docker base). One rasterizer covers PNG now + PDF later.
- _Alt — resvg (`resvg-dotnet`/native):_ best SVG fidelity, but a non-NuGet native dep to vendor per-arch. Reject for ops cost.
- _Alt — headless Chrome (Playwright):_ perfect parity, heavyweight runtime + cold-start in the render path. Reject (revisit only if Skia SVG fidelity disappoints).
- _Alt — keep QRCoder `PngByteQRCode`:_ can't raster a styled SVG → drops shapes/gradients on PNG. Reject.
- Logo on PNG = baked in the SVG (knockout + `<image>`) → rasterizer renders it; retire the ImageSharp post-overlay for styled output.

### 3. Preview parity — frontend previews the backend-emitted SVG (DECISION)

- Live/debounced endpoint returns the **emitter's** SVG for the in-progress `StyleSpec`; builder renders that exact SVG. Same bytes as export by construction.
- Drop `qrcode.react` from the styled preview (keep only as an offline placeholder, if at all).
- _Alt — port the emitter to TS (client mirror):_ re-introduces drift + double the surface. Reject.
- Endpoint shape (open Q): `POST /api/codes/preview {payload, styleSpec}` (unsaved) vs reuse `GET /{id}/image` once saved → likely a stateless `preview` endpoint for the builder + the existing image endpoint for saved codes.

### 4. StyleSpec — one extensible record, the contract

- A versioned `StyleSpec` record persisted to `CodeEntity.StyleJson`; the emitter's only style input. Future features = new fields, not new code paths:
  - `Modules { Shape, ColorOrGradient }` · `FinderEye { FrameShape, FrameColor, PupilShape, PupilColor }` · `Background { Color, Transparent }` · `Gradient { Kind, Stops, Target }` · `Logo { Bytes/Ref, Scale, Knockout }` · `Frame { Style, Caption, CaptionColor }` · `QuietZoneModules` · `Ecc` · `Format`.
- `SchemaVersion` field for forward migration. Validation (contrast floor, quiet-zone floor, ECC auto-bump) runs over the spec **before** emit (the contrast validator is a v0.5 must-add dep).
- Render request becomes `(payload, symbology, StyleSpec, Format)`; `CodeImageService` deserializes `StyleJson` → `StyleSpec` (currently passes nothing).

## Task list (ordered, one-liner each)

1. Define `StyleSpec` record (+ `SchemaVersion`, sub-records above) in `SmartQr.Codes/Models/`; default = today's plain black/white.
2. Add a matrix abstraction — expose QRCoder QR + ZXing as a plain `BitMatrix`/module grid the emitter consumes.
3. Build the custom `SvgRenderer` — square modules first (parity baseline), then `<rect>`/quiet-zone/fg/bg/ECC off `StyleSpec`.
4. Swap `QrCodeRenderer.RenderSvg` to the emitter; keep output identical for the default spec (regression gate).
5. Add Svg.Skia rasterizer — `SvgRenderer` SVG → PNG; wire `SkiaSharp.NativeAssets.Linux` + Docker base; retire `PngByteQRCode` for the styled path.
6. Re-home logo into the emitter (knockout modules + `<image>`); raster picks it up; retire the ImageSharp post-overlay for styled renders.
7. Thread `StyleSpec` end-to-end — `CodeRenderRequest` → `CodeImageService` reads `CodeEntity.StyleJson` → renderer.
8. Add the builder preview endpoint (`POST /api/codes/preview` → emitter SVG for an unsaved `StyleSpec`).
9. Point `QrPreview` at the backend SVG (debounced); remove the `qrcode.react` styled path.
10. Pre-emit validation hook — quiet-zone floor + ECC auto-bump (+ contrast-validator seam for the later styling iteration).
11. Unit + E2E — default-spec byte-parity (emitter == old output) · PNG raster on Linux · preview endpoint == image endpoint for the same spec.

## Open questions (for the user)

1. **Rasterizer:** lock **Svg.Skia (SkiaSharp)** vs prefer **resvg** (higher SVG fidelity, native-vendor cost) vs **headless Chrome** (perfect parity, heavy)? Recommend Svg.Skia.
2. **Preview endpoint:** stateless `POST /codes/preview` for unsaved specs + keep `GET /{id}/image` for saved — agreed? Or preview saved-only.
3. **Scope split:** this iteration = emitter + raster + parity + `StyleSpec` model + **solid colors / bg / transparency / logo / ECC / quiet-zone** only; shapes · finder-eye · gradients · frames · presets land in the **Code styling** iteration on top. Confirm the cut.
4. **Barcodes:** keep ZXing's plain SVG for 1D/2D this iteration (no styling), unify under the emitter later — OK?
5. **Logo on SVG:** ship module knockout now (was a "V2" item) or simple `<image>` overlay first, knockout in styling iteration?
6. **`StyleJson` shape:** persist the full `StyleSpec` JSON (camelCase) in the existing JSONB column — no migration. Confirm no separate columns wanted.
