# Architecture — Code Generation

*Last updated: 2026-06-03*

## Purpose

Render a code (QR or barcode) to SVG or PNG from a payload (typically a code's short URL). Vector-first, cross-platform, styleable. Lives in the standalone `SmartQr.Codes` library so it's reusable across services (and extractable as `Wow.Two.Sdk.Beta.Codes` later).

## How it works

```
CodeRenderRequest (payload, symbology, format, options)
        ↓
ICodeRenderer → CodeRenderer (facade)
        ├─ QR     → QrCodeRenderer (QRCoder: SvgQRCode / PngByteQRCode)
        │             └─ optional center logo → ImageSharpLogoCompositor (PNG only)
        └─ barcode → BarcodeRenderer (ZXing.Net → SVG)
        ↓
RenderedCode (bytes, content-type, format)
```

- **QR matrix is the source of truth**; SVG/PNG are renders off the same matrix. SVG = default (vector, infinite print scale, re-styleable, ~1–10 KB); PNG = raster export.
- **Cross-platform / no `System.Drawing`** — only QRCoder's `SvgQRCode` + `PngByteQRCode` renderers are used (Linux-safe). Barcodes use ZXing's managed SVG renderer. Logo overlay uses ImageSharp (managed).
- **Error correction** defaults to **Q** so a center logo can occlude the middle and the code still scans.

## Key types / files

| Type | File |
|---|---|
| `ICodeRenderer` | `platform/src/backend/SmartQr.Codes/ICodeRenderer.cs` |
| `CodeRenderer` (facade/dispatch) | `SmartQr.Codes/Rendering/CodeRenderer.cs` |
| `QrCodeRenderer` | `SmartQr.Codes/Rendering/QrCodeRenderer.cs` |
| `BarcodeRenderer` | `SmartQr.Codes/Rendering/BarcodeRenderer.cs` |
| `ImageSharpLogoCompositor` | `SmartQr.Codes/Logo/ImageSharpLogoCompositor.cs` |
| `CodeRenderRequest` / `CodeRenderOptions` / `RenderedCode` / `EccLevel` | `SmartQr.Codes/Models/` |
| `BarcodeFormat` / `ImageFormat` | `SmartQr.Common.Domain/Codes/Enums/` |
| DI: `AddSmartQrCodes()` | `SmartQr.Codes/ServiceCollectionExtensions.cs` |

Consumed by the API via `ICodeImageService` (`SmartQr.Api/Infrastructure/Codes/Services/CodeImageService.cs`).

## Decisions & tradeoffs

- **QRCoder** for QR (best payload generators, managed), **ZXing.Net** for the rest (1D + 2D coverage). Two libs, one facade.
- **ImageSharp 2.1.x (Apache-2.0)** for logo compositing — managed + license-clean (vs SkiaSharp's native assets).
- Renderers are **stateless singletons** (thread-safe).

## Edge cases

- Logo only composited on **PNG** (raster); SVG logo embedding is V2. Logo present → keep EC at Q/H.
- Barcode **PNG** export not implemented (SVG only) — needs a raster binding; V2.
- GIF/animated export (V2) will band on gradients in 256-color GIF → prefer animated WebP/MP4. See spec §5c.

## Open questions

- Add PDF/EPS export (vector — trivial off the matrix via QRCoder `PdfByteQRCode`)?
- Logo/pfp **circular knockout** (clear modules behind the image) vs simple overlay — knockout reads cleaner (Telegram-style).
- Animated formats: GIF (universal) + WebP (smaller) via ImageSharp; MP4 via FFmpeg; Lottie/animated-SVG for web.
