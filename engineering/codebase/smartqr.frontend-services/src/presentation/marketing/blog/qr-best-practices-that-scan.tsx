import { type Post } from "./types";

export const qrBestPracticesThatScan: Post = {
  meta: {
    slug: "qr-best-practices-that-scan",
    title: "QR code best practices: sizes, error correction, logos and contrast that actually scan",
    description:
      "A QR code that looks great but won't scan is worthless. Here are the practical rules — quiet zone, error correction, minimum size, contrast, and logos — that keep codes readable from a phone.",
    date: "2026-06-10",
    readingMinutes: 7,
    tag: "How-to",
  },
  Body: () => (
    <>
      <p>
        Most QR-code failures aren't bad luck — they're predictable. A code printed too small, with too
        little contrast, no quiet zone, or a logo that swallows the wrong part of the pattern simply
        won't decode. Here's the short list of things that actually govern whether a phone can read your
        code.
      </p>

      <h2>1. Leave the quiet zone</h2>
      <p>
        A QR code needs a margin of empty space around it — the "quiet zone" — so the scanner can tell
        where the code starts and stops. The standard is <strong>four modules wide</strong> (a "module"
        is one of the small squares). Crowd the code with text or artwork right up to its edge and decode
        rates drop fast. When in doubt, give it more whitespace, not less.
      </p>

      <h2>2. Size it for the scanning distance</h2>
      <p>
        The rule of thumb is a <strong>10:1 distance-to-size ratio</strong>: for every 10 units of
        scanning distance, the code needs about 1 unit of width. A code scanned from 30&nbsp;cm away
        (a table tent or business card) wants to be at least ~3&nbsp;cm; a poster read from 3&nbsp;m
        needs to be ~30&nbsp;cm. Billboards need to be huge. Err larger — a code that's slightly too big
        always scans; one that's slightly too small never does.
      </p>

      <h2>3. Mind contrast and color</h2>
      <ul>
        <li>
          <strong>Dark on light, not the reverse.</strong> Scanners expect dark modules on a light
          background. Inverting it (light code on a dark background) breaks many readers.
        </li>
        <li>
          <strong>Keep strong contrast.</strong> A pale code on a near-white background or a low-contrast
          gradient is a coin flip. Aim for the same kind of contrast you'd want for readable text.
        </li>
        <li>
          <strong>Watch the substrate.</strong> Glossy stock, transparent packaging, fabric, and curved
          surfaces all reduce reliability. Test on the real material, not your screen.
        </li>
      </ul>

      <h2>4. Understand error correction</h2>
      <p>
        QR codes have built-in redundancy: a portion of the data is repeated so the code still decodes
        even if part of it is dirty, scratched, or covered. There are four levels:
      </p>
      <ul>
        <li>
          <strong>L</strong> — recovers ~7% of the code
        </li>
        <li>
          <strong>M</strong> — ~15%
        </li>
        <li>
          <strong>Q</strong> — ~25%
        </li>
        <li>
          <strong>H</strong> — ~30%
        </li>
      </ul>
      <p>
        Higher correction means a denser, busier code (more modules) but more resilience. For plain
        codes, <strong>M</strong> is a fine default. The moment you add a center logo, step up to{" "}
        <strong>Q or H</strong> so the code can survive the part the logo covers.
      </p>

      <h2>5. Add a logo without breaking the code</h2>
      <p>A center logo is the most common way to brand a code — and the most common way to break one.</p>
      <ul>
        <li>
          <strong>Keep it centered and small.</strong> Cover roughly the middle 20–30% at most. The
          three big corner squares (the "finder patterns") must stay untouched — they're how the scanner
          locks on.
        </li>
        <li>
          <strong>Raise error correction to H.</strong> The redundancy is what lets the code tolerate the
          area the logo occludes.
        </li>
        <li>
          <strong>Use a clean knockout.</strong> Clear a small circular or square area for the logo
          rather than laying it on top of live modules, so the surrounding pattern stays crisp.
        </li>
      </ul>

      <h2>6. Export in the right format</h2>
      <p>
        Prefer a <strong>vector format (SVG or PDF)</strong> for print — it scales from a business card
        to a billboard with zero quality loss, because the code is regenerated cleanly at any size. Use{" "}
        <strong>PNG</strong> for screens and for the many systems that don't accept SVG. Avoid JPEG: its
        compression adds fuzzy artifacts around the sharp module edges that scanners rely on.
      </p>
      <p>
        Note that format alone doesn't make a code scannable — quiet zone, contrast, size, and error
        correction do. A vector export just preserves that fidelity at any scale.
      </p>

      <h2>7. Always test before you print at scale</h2>
      <p>This is the step everyone skips and everyone regrets. Before committing to 10,000 copies:</p>
      <ul>
        <li>Scan it with the native camera on both a recent iPhone and an Android phone.</li>
        <li>Try a third-party scanner app too — they're less forgiving than native cameras.</li>
        <li>Test on the actual printed material, at the actual size, under the actual lighting.</li>
        <li>If it's a dynamic code, confirm the destination is correct — and that you can change it later.</li>
      </ul>

      <h2>The short version</h2>
      <p>
        Give the code a quiet zone, make it big enough for its distance, keep it dark-on-light with
        strong contrast, use error-correction level H whenever there's a logo, export as vector for
        print, and test on real hardware before you commit. Do those six things and your codes will
        scan on the first try — every time.
      </p>
    </>
  ),
};
