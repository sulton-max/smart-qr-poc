import { useEffect, useRef } from "react";
import QRCode from "qrcode";
import { HERO_CODE_URLS } from "./heroCodes";

export type HeroMode = "drift" | "bump" | "chase";

/** Live-tunable simulation parameters (read per-frame, so slider changes take effect instantly). */
export interface SimParams {
  count: number;
  /** Card alpha. */
  opacity: number;
  /** Drift cruise speed. */
  driftSpeed: number;
  /** Bump charge speed. */
  ramSpeed: number;
  /** Chase seek speed. */
  seekSpeed: number;
  /** Chase dead-zone bubble radius (px) around the cursor. */
  mouseGap: number;
}

export const DEFAULT_PARAMS: SimParams = {
  count: 26,
  opacity: 0.92,
  driftSpeed: 46,
  ramSpeed: 250,
  seekSpeed: 210,
  mouseGap: 50,
};

interface Sprite {
  x: number;
  y: number;
  vx: number;
  vy: number;
  rot: number;
  vrot: number;
  /** Doubles as the collision mass — bigger codes shove smaller ones in `bump`. */
  size: number;
  tex: number;
  /** In `bump`, a random waypoint this sprite roams toward (kills the center-brawl). */
  tx: number;
  ty: number;
  /** In `bump`, while `now < restUntil` the sprite drops attacking and just drifts. */
  restUntil: number;
}

const TILE = 132; // baked-texture px (QR + white tile + padding)
const CLASH = 0.42; // collision min-distance factor — tight (no artificial gap between codes)
const BUMP_E = 0.92; // bump restitution (angry, near-elastic)
const FRAME_MS = 1000 / 60; // update-rate cap — 60fps for smooth motion; also caps 120Hz displays to 60

function roundRect(ctx: CanvasRenderingContext2D, x: number, y: number, w: number, h: number, r: number): void {
  ctx.beginPath();
  ctx.moveTo(x + r, y);
  ctx.arcTo(x + w, y, x + w, y + h, r);
  ctx.arcTo(x + w, y + h, x, y + h, r);
  ctx.arcTo(x, y + h, x, y, r);
  ctx.arcTo(x, y, x + w, y, r);
  ctx.closePath();
}

/** Bake each URL into a floating "card": a real QR on a rounded white tile with a violet-tinted shadow. */
async function bakeTextures(urls: readonly string[]): Promise<HTMLCanvasElement[]> {
  const out: HTMLCanvasElement[] = [];
  for (const url of urls) {
    const qr = document.createElement("canvas");
    await QRCode.toCanvas(qr, url, {
      margin: 1,
      width: 100,
      errorCorrectionLevel: "M",
      color: { dark: "#18181b", light: "#ffffff" },
    });

    const tile = document.createElement("canvas");
    tile.width = TILE;
    tile.height = TILE;
    const ctx = tile.getContext("2d");
    if (!ctx) continue;

    const pad = 14;
    const inner = TILE - pad * 2;
    ctx.save();
    ctx.shadowColor = "rgba(109,40,217,0.20)";
    ctx.shadowBlur = 16;
    ctx.shadowOffsetY = 5;
    roundRect(ctx, pad, pad, inner, inner, 16);
    ctx.fillStyle = "#ffffff";
    ctx.fill();
    ctx.restore();

    roundRect(ctx, pad, pad, inner, inner, 16);
    ctx.strokeStyle = "rgba(109,40,217,0.16)";
    ctx.lineWidth = 1;
    ctx.stroke();

    const q = inner - 14;
    ctx.drawImage(qr, pad + 7, pad + 7, q, q);
    out.push(tile);
  }
  return out;
}

interface HeroCanvasProps {
  mode: HeroMode;
  params: SimParams;
}

/**
 * Full-bleed canvas of floating, real, scannable QR "cards" behind the hero. Clash detection is
 * always on (codes never overlap; no gap). Modes: `drift` (calm cruise + DVD-style wall bounce;
 * click pushes codes away), `bump` (angry — roam random waypoints + weight-based ramming; up to ⅓
 * randomly "rest" and drift for 2-3s; click = a 1s truce), `chase` (gather around the cursor).
 *
 * Perf: DPR-capped, ~40fps update cap, and paused via `IntersectionObserver` when the hero scrolls
 * off-screen + on hidden tab; sprite count auto-reduces on narrow viewports. Respects
 * `prefers-reduced-motion` (a single settled static scatter). Physics read `params` per-frame.
 */
export function HeroCanvas({ mode, params }: HeroCanvasProps) {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const modeRef = useRef<HeroMode>(mode);
  modeRef.current = mode;
  const paramsRef = useRef<SimParams>(params);
  paramsRef.current = params;

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext("2d");
    if (!ctx) return;

    const reduce = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    const dpr = Math.min(window.devicePixelRatio || 1, 1.5);
    const pointer = { x: -9999, y: -9999, active: false };
    let sprites: Sprite[] = [];
    let textures: HTMLCanvasElement[] = [];
    let raf = 0;
    let last = performance.now();
    let disposed = false;
    let started = false;
    let paused = false;
    let onScreen = true;
    // Optimistic — a real tab-switch flips this via `visibilitychange`; background-tab rAF is throttled
    // anyway. (Avoids a frozen start in headless/embedded contexts that report `hidden` at load.)
    let tabVisible = true;
    let lastW = 0;
    let calmUntil = 0; // bump: truce end timestamp (ms)

    function viewport(): { w: number; h: number } {
      const rect = canvas!.getBoundingClientRect();
      return { w: rect.width, h: rect.height };
    }

    function resize(): void {
      const { w, h } = viewport();
      canvas!.width = Math.round(w * dpr);
      canvas!.height = Math.round(h * dpr);
      ctx!.setTransform(dpr, 0, 0, dpr, 0, 0);
    }

    /** Fewer sprites on smaller viewports (mobile/tablet) → less fill + fewer collision pairs. */
    function effectiveCount(): number {
      const want = Math.round(paramsRef.current.count);
      const { w } = viewport();
      const byWidth = w < 560 ? Math.min(want, 10) : w < 900 ? Math.min(want, 18) : want;
      return reduce ? Math.min(12, byWidth) : byWidth;
    }

    function makeSprite(w: number, h: number): Sprite {
      return {
        x: Math.random() * w,
        y: Math.random() * h,
        vx: (Math.random() - 0.5) * 30,
        vy: (Math.random() - 0.5) * 30,
        rot: (Math.random() - 0.5) * 0.6,
        vrot: (Math.random() - 0.5) * 0.25,
        size: 44 + Math.random() * 46,
        tex: Math.floor(Math.random() * Math.max(1, textures.length)),
        tx: Math.random() * w,
        ty: Math.random() * h,
        restUntil: 0,
      };
    }

    function spawn(): void {
      const { w, h } = viewport();
      sprites = Array.from({ length: effectiveCount() }, () => makeSprite(w, h));
    }

    /** Add/remove sprites to match the live count without disturbing the rest. */
    function reconcileCount(): void {
      const target = effectiveCount();
      const { w, h } = viewport();
      while (sprites.length < target) sprites.push(makeSprite(w, h));
      if (sprites.length > target) sprites.length = target;
    }

    function draw(): void {
      const { w, h } = viewport();
      ctx!.clearRect(0, 0, w, h);
      const alpha = paramsRef.current.opacity;
      for (const s of sprites) {
        const t = textures[s.tex];
        if (!t) continue;
        ctx!.save();
        ctx!.translate(s.x, s.y);
        ctx!.rotate(s.rot);
        ctx!.globalAlpha = alpha;
        ctx!.drawImage(t, -s.size / 2, -s.size / 2, s.size, s.size);
        ctx!.restore();
      }
    }

    function step(dt: number): void {
      const { w, h } = viewport();
      const m = modeRef.current;
      const p = paramsRef.current;
      const now = performance.now();
      const calm = m === "bump" && now < calmUntil;

      // Bump: randomly send up to ⅓ of the codes on a 2-3s "break" (they drop attacking + just drift).
      if (m === "bump" && !calm) {
        let resting = 0;
        for (const s of sprites) if (now < s.restUntil) resting++;
        if (resting < Math.floor(sprites.length / 3) && Math.random() < 0.05) {
          const start = Math.floor(Math.random() * sprites.length);
          for (let k = 0; k < sprites.length; k++) {
            const s = sprites[(start + k) % sprites.length];
            if (now >= s.restUntil) {
              s.restUntil = now + 2000 + Math.random() * 1000;
              break;
            }
          }
        }
      }

      for (const s of sprites) {
        if (m === "chase" && pointer.active) {
          // Seek the cursor but stop at a dead-zone ring → they GATHER around it, not pile on the point.
          const dx = pointer.x - s.x;
          const dy = pointer.y - s.y;
          const d = Math.hypot(dx, dy) || 1;
          const ring = p.mouseGap + s.size * 0.5;
          if (d > ring) {
            s.vx += ((dx / d) * p.seekSpeed - s.vx) * Math.min(1, dt * 2.6);
            s.vy += ((dy / d) * p.seekSpeed - s.vy) * Math.min(1, dt * 2.6);
          } else {
            const out = (ring - d) * 6;
            s.vx += (-dx / d) * out * dt - s.vx * 0.14;
            s.vy += (-dy / d) * out * dt - s.vy * 0.14;
          }
        } else if (m === "bump" && !calm && now >= s.restUntil) {
          // Attacking: roam toward a random waypoint (spreads them); ram whatever's in the way.
          const dx = s.tx - s.x;
          const dy = s.ty - s.y;
          const d = Math.hypot(dx, dy) || 1;
          if (d < 46) {
            s.tx = Math.random() * w;
            s.ty = Math.random() * h;
          }
          s.vx += ((dx / d) * p.ramSpeed - s.vx) * Math.min(1, dt * 1.5);
          s.vy += ((dy / d) * p.ramSpeed - s.vy) * Math.min(1, dt * 1.5);
        } else if (m === "bump" && calm) {
          // Truce — coast to a near-stop, then bumping resumes when it lifts.
          s.vx *= 0.9;
          s.vy *= 0.9;
        } else {
          // Drift (and bump "resting" sprites): cruise gently toward driftSpeed, preserving direction.
          const sp = Math.hypot(s.vx, s.vy) || 1;
          s.vx += ((s.vx / sp) * p.driftSpeed - s.vx) * 0.02;
          s.vy += ((s.vy / sp) * p.driftSpeed - s.vy) * 0.02;
        }

        s.x += s.vx * dt;
        s.y += s.vy * dt;
        s.rot += s.vrot * dt;
        s.vrot *= 0.99;

        const r = s.size * 0.5;
        if (s.x < r) {
          s.x = r;
          s.vx = Math.abs(s.vx);
        } else if (s.x > w - r) {
          s.x = w - r;
          s.vx = -Math.abs(s.vx);
        }
        if (s.y < r) {
          s.y = r;
          s.vy = Math.abs(s.vy);
        } else if (s.y > h - r) {
          s.y = h - r;
          s.vy = -Math.abs(s.vy);
        }
      }

      // Clash detection — always on. In `bump`, mass = size (bigger shoves smaller); elsewhere equal + gentle.
      for (let i = 0; i < sprites.length; i++) {
        for (let j = i + 1; j < sprites.length; j++) {
          const a = sprites[i];
          const b = sprites[j];
          const dx = b.x - a.x;
          const dy = b.y - a.y;
          const dist = Math.hypot(dx, dy) || 1;
          const min = (a.size + b.size) * CLASH;
          if (dist >= min) continue;
          const nx = dx / dist;
          const ny = dy / dist;
          const push = min - dist;

          if (m === "bump") {
            const invA = 1 / a.size;
            const invB = 1 / b.size;
            const invSum = invA + invB;
            a.x -= nx * push * (invA / invSum);
            a.y -= ny * push * (invA / invSum);
            b.x += nx * push * (invB / invSum);
            b.y += ny * push * (invB / invSum);
            const rel = (b.vx - a.vx) * nx + (b.vy - a.vy) * ny;
            if (rel < 0) {
              const jimp = (-(1 + BUMP_E) * rel) / invSum;
              a.vx -= jimp * invA * nx;
              a.vy -= jimp * invA * ny;
              b.vx += jimp * invB * nx;
              b.vy += jimp * invB * ny;
            }
            a.vrot += (Math.random() - 0.5) * 0.6;
            b.vrot += (Math.random() - 0.5) * 0.6;
            a.tx = Math.random() * w;
            a.ty = Math.random() * h;
            b.tx = Math.random() * w;
            b.ty = Math.random() * h;
          } else {
            const overlap = push / 2;
            a.x -= nx * overlap;
            a.y -= ny * overlap;
            b.x += nx * overlap;
            b.y += ny * overlap;
            const avn = a.vx * nx + a.vy * ny;
            const bvn = b.vx * nx + b.vy * ny;
            const diff = (bvn - avn) * 0.35;
            a.vx += diff * nx;
            a.vy += diff * ny;
            b.vx -= diff * nx;
            b.vy -= diff * ny;
          }
        }
      }

      // Speed caps (scale with the tuned speed so sliders can push it).
      const cap = m === "bump" ? p.ramSpeed * 1.5 : m === "chase" ? p.seekSpeed * 1.3 : p.driftSpeed * 2.2;
      for (const s of sprites) {
        const sp = Math.hypot(s.vx, s.vy);
        if (sp > cap) {
          s.vx = (s.vx / sp) * cap;
          s.vy = (s.vy / sp) * cap;
        }
      }
    }

    function frame(now: number): void {
      if (disposed || paused) {
        raf = 0;
        return;
      }
      raf = requestAnimationFrame(frame);
      const elapsed = now - last;
      if (elapsed < FRAME_MS) return; // cap the update rate — cheap early-out keeps CPU/GPU down
      last = now - (elapsed % FRAME_MS);
      reconcileCount();
      step(Math.min(0.05, elapsed / 1000));
      draw();
    }

    /** Single control: run only when the hero is on-screen, the tab is visible, and motion is allowed. */
    function updatePaused(): void {
      let visible = onScreen;
      if (!visible) {
        // Guard a flaky IntersectionObserver (embedded / headless contexts): trust the real rect.
        const r = canvas!.getBoundingClientRect();
        const vh = window.innerHeight || document.documentElement.clientHeight || 0;
        visible = r.width > 0 && r.bottom > 0 && r.top < vh;
      }
      const should = visible && tabVisible && started && !reduce && !disposed;
      paused = !should;
      if (!paused && raf === 0) {
        last = performance.now();
        raf = requestAnimationFrame(frame);
      }
    }

    function onPointerMove(e: PointerEvent): void {
      const rect = canvas!.getBoundingClientRect();
      pointer.x = e.clientX - rect.left;
      pointer.y = e.clientY - rect.top;
      pointer.active = true;
    }
    function onPointerLeave(): void {
      pointer.active = false;
    }
    function onPointerDown(e: PointerEvent): void {
      // Ignore clicks on the UI (dev panel, CTAs, nav) — only empty hero spots drive the sim.
      if (e.target instanceof Element && e.target.closest("a,button,select,input,label")) return;
      const rect = canvas!.getBoundingClientRect();
      const x = e.clientX - rect.left;
      const y = e.clientY - rect.top;
      if (x < 0 || y < 0 || x > rect.width || y > rect.height) return;
      const m = modeRef.current;
      if (m === "drift") {
        // Medium radial shove outward from the click, falling off with distance.
        const R = 260;
        const F = 360;
        for (const s of sprites) {
          const dx = s.x - x;
          const dy = s.y - y;
          const d = Math.hypot(dx, dy) || 1;
          if (d < R) {
            const f = (1 - d / R) * F;
            s.vx += (dx / d) * f;
            s.vy += (dy / d) * f;
          }
        }
      } else if (m === "bump") {
        calmUntil = performance.now() + 1100; // ~1s truce, then they resume
      }
    }
    function onVisibility(): void {
      tabVisible = !document.hidden;
      updatePaused();
    }

    const io = new IntersectionObserver(
      (entries) => {
        onScreen = entries[0]?.isIntersecting ?? true;
        updatePaused();
      },
      { threshold: 0 },
    );

    function handleSize(): void {
      const { w, h } = viewport();
      if (w < 1 || h < 1) return;
      if (sprites.length === 0 || Math.abs(w - lastW) > 2) {
        resize();
        spawn();
        lastW = w;
      }
      if (reduce) {
        for (let k = 0; k < 40; k++) step(1 / 60); // settle so the static scatter doesn't overlap
        draw();
        return;
      }
      if (!started) {
        started = true;
        window.addEventListener("pointermove", onPointerMove);
        window.addEventListener("pointerdown", onPointerDown);
        canvas!.addEventListener("pointerleave", onPointerLeave);
        document.addEventListener("visibilitychange", onVisibility);
        io.observe(canvas!);
      }
      updatePaused();
    }

    const ro = new ResizeObserver(handleSize);

    void (async () => {
      textures = await bakeTextures(HERO_CODE_URLS);
      if (disposed) return;
      ro.observe(canvas);
      handleSize();
    })();

    return () => {
      disposed = true;
      ro.disconnect();
      io.disconnect();
      cancelAnimationFrame(raf);
      window.removeEventListener("pointermove", onPointerMove);
      window.removeEventListener("pointerdown", onPointerDown);
      canvas.removeEventListener("pointerleave", onPointerLeave);
      document.removeEventListener("visibilitychange", onVisibility);
    };
  }, []);

  return <canvas ref={canvasRef} className="absolute inset-0 h-full w-full" aria-hidden="true" />;
}
