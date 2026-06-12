import {
  BarChart3,
  Globe,
  Infinity as InfinityIcon,
  Pencil,
  Printer,
  QrCode,
  RefreshCw,
  Route,
  Sparkles,
  type LucideIcon,
} from "lucide-react";

/**
 * Single source of truth for the marketing surface — pricing, features, the comparison table, the
 * how-it-works steps, and FAQs. The landing page renders compact slices of this; the dedicated
 * pricing page renders it in full. Keep prices in sync with the spec (`ideas/smart-qr-spec.md` §9)
 * and `business/business-knowledge.md`.
 */

export const BRAND = {
  name: "Smart QR",
  tagline: "Programmable codes that never expire.",
  pitch:
    "One scannable code, many destinations by context — and you'll never have to reprint it.",
} as const;

// ── Features (landing grid + features section) ──────────────────────────────

export interface Feature {
  icon: LucideIcon;
  title: string;
  body: string;
}

export const FEATURES: Feature[] = [
  {
    icon: Route,
    title: "Programmable routing",
    body: "One code, many destinations. Route by device, country, language, time of day, scan count, or an A/B split — first matching rule wins, with a fallback so every scan lands somewhere.",
  },
  {
    icon: InfinityIcon,
    title: "Codes never expire",
    body: "Print once, reprogram forever. Your codes keep resolving even on a free or lapsed plan — we never brick the flyers you already paid to print.",
  },
  {
    icon: QrCode,
    title: "Every code type",
    body: "QR, 1D & 2D barcodes (Code 128, EAN/UPC, Data Matrix, PDF417, Aztec), short links, vCard cards and WiFi join — one surface instead of five subscriptions.",
  },
  {
    icon: BarChart3,
    title: "Calm analytics",
    body: "Scans over time, unique vs. repeat, by device, OS and country. Pull it when you want it — no ad emails, no \"you got a scan!\" nags.",
  },
  {
    icon: Globe,
    title: "Custom domain from $5",
    body: "Serve your codes from qr.yourbrand.com on the $5 tier — not gated behind a $30+ enterprise plan like everywhere else.",
  },
  {
    icon: Sparkles,
    title: "Built to be left alone",
    body: "Unlimited scans on every tier. No scan caps as a paywall lever. Cancel, export everything, and delete everything — anytime.",
  },
];

// ── How it works (3 steps) ──────────────────────────────────────────────────

export interface Step {
  icon: LucideIcon;
  title: string;
  body: string;
}

export const STEPS: Step[] = [
  {
    icon: Pencil,
    title: "Create",
    body: "Pick a destination, style your code, and add routing rules in a visual builder. No account needed to start.",
  },
  {
    icon: Printer,
    title: "Print",
    body: "Export a crisp vector SVG, PNG, or PDF and put it on anything — a table tent, a billboard, product packaging.",
  },
  {
    icon: RefreshCw,
    title: "Reprogram",
    body: "Change where it points whenever you like. The printed code keeps working — forever, on any plan.",
  },
];

// ── Pricing tiers ───────────────────────────────────────────────────────────

export interface PricingTier {
  id: string;
  name: string;
  price: string;
  cadence: string;
  tagline: string;
  /** Visually emphasised as the recommended plan. */
  featured?: boolean;
  cta: string;
  features: string[];
}

export const PRICING: PricingTier[] = [
  {
    id: "free",
    name: "Free",
    price: "$0",
    cadence: "forever",
    tagline: "For tinkerers and first codes.",
    cta: "Start free",
    features: [
      "3 dynamic codes",
      "Unlimited scans",
      "Basic analytics",
      "Codes never expire",
    ],
  },
  {
    id: "solo",
    name: "Solo",
    price: "$5",
    cadence: "/mo",
    tagline: "For a small business with physical surfaces.",
    featured: true,
    cta: "Choose Solo",
    features: [
      "25 codes",
      "Smart routing rules",
      "Custom domain",
      "Full analytics",
      "Everything in Free",
    ],
  },
  {
    id: "pro",
    name: "Pro",
    price: "$15",
    cadence: "/mo",
    tagline: "For marketers and power users.",
    cta: "Choose Pro",
    features: [
      "200 codes",
      "A/B + advanced rules",
      "Bulk generation",
      "All code & content types",
      "Everything in Solo",
    ],
  },
  {
    id: "agency",
    name: "Dev / Agency",
    price: "$39",
    cadence: "/mo",
    tagline: "For developers and agencies.",
    cta: "Choose Agency",
    features: [
      "REST API + keys",
      "White-label",
      "Client workspaces",
      "Webhooks",
      "Everything in Pro",
    ],
  },
];

// ── Comparison (anti-incumbent) ─────────────────────────────────────────────

export interface ComparisonRow {
  dimension: string;
  smartQr: string;
  incumbent: string;
}

export const COMPARISON: ComparisonRow[] = [
  {
    dimension: "Codes when you downgrade",
    smartQr: "Keep working, forever",
    incumbent: "Often deactivated — your print is bricked",
  },
  {
    dimension: "Scans",
    smartQr: "Unlimited on every tier",
    incumbent: "Metered, capped as a paywall lever",
  },
  {
    dimension: "Smart routing + custom domain",
    smartQr: "From $5/mo",
    incumbent: "Buried in $30+/mo tiers",
  },
  {
    dimension: "Developer API at a flat price",
    smartQr: "Yes — no sales call",
    incumbent: "Enterprise-only or absent",
  },
  {
    dimension: "Email behaviour",
    smartQr: "No ads, no scan nags",
    incumbent: "Engagement emails by default",
  },
  {
    dimension: "Export & delete everything",
    smartQr: "Anytime, one click",
    incumbent: "Friction or unavailable",
  },
];

// ── FAQ ─────────────────────────────────────────────────────────────────────

export interface Faq {
  q: string;
  a: string;
}

export const FAQS: Faq[] = [
  {
    q: "What's a dynamic QR code?",
    a: "A QR code whose destination you can change after it's printed. The image encodes a short link on our domain; you reprogram where that link forwards to without ever reprinting the code.",
  },
  {
    q: "Do my codes really never expire?",
    a: "Yes. The redirect keeps working even on the free tier or a lapsed plan. If you stop paying you lose editing and analytics — never the redirect itself. We never deactivate a printed code.",
  },
  {
    q: "What is smart routing?",
    a: "An ordered set of rules on a single code: if the scanner's device, country, language, the time of day, scan count, or an A/B bucket matches, send them to one destination — otherwise fall back to your default. One code behaves differently for whoever scans it.",
  },
  {
    q: "Can I change where a code points after printing?",
    a: "Anytime, instantly, with no reprint. That's the whole point of a dynamic code — the printed image is permanent, the destination is not.",
  },
  {
    q: "Which code types are supported?",
    a: "QR codes, 1D and 2D barcodes (Code 128, EAN-13/UPC-A, Data Matrix, PDF417, Aztec), plain short links, vCard digital business cards, and WiFi-join codes — all from one place.",
  },
  {
    q: "Is there really a free plan?",
    a: "Yes — three dynamic codes, unlimited scans, basic analytics, and the never-expire promise, at no cost and with no card required.",
  },
];

export const PRICING_FAQS: Faq[] = [
  {
    q: "What happens to my codes if I downgrade or cancel?",
    a: "They keep redirecting. You lose the ability to edit them and see new analytics, but every printed code continues to resolve to its last destination — permanently. This is the opposite of how most QR vendors work.",
  },
  {
    q: "Are scans really unlimited on every plan?",
    a: "Yes, including Free. We don't use scan caps as a paywall lever. A code that goes viral won't get throttled or hit a surprise bill.",
  },
  {
    q: "Do I need a credit card to start?",
    a: "No. Start as a guest, create your first codes, and only add billing when you want more codes or the paid routing features.",
  },
  {
    q: "Can I bring my own domain?",
    a: "Yes — from the $5 Solo tier. Point a CNAME like qr.yourbrand.com at us and your codes resolve through your own domain, not a shared one.",
  },
  {
    q: "Is there a discount for paying yearly?",
    a: "Annual pricing is coming alongside billing. The flat monthly prices shown here are the anchor — no per-scan fees, no hidden metering.",
  },
];
