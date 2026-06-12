import { type ReactNode } from "react";
import { useState } from "react";
import { Link } from "react-router-dom";
import { Button } from "@wow-two-beta/ui/actions";
import {
  ArrowRight,
  Check,
  ChevronDown,
  Clock,
  Globe,
  Languages,
  QrCode,
  Smartphone,
  X,
} from "lucide-react";
import { QrPreview } from "../components/QrPreview";
import { COMPARISON, type Faq, type Feature, PRICING, type PricingTier, type Step } from "./data";
import { type PostMeta } from "./blog/types";

/** Brand mark — violet QR tile + wordmark. Shared by the marketing header/footer and the app shell. */
export function Logo({ className = "" }: { className?: string }) {
  return (
    <span className={`inline-flex shrink-0 items-center gap-2 font-bold ${className}`}>
      <span className="grid size-7 shrink-0 place-items-center rounded-md bg-primary text-primary-foreground">
        <QrCode size={18} />
      </span>
      <span className="whitespace-nowrap text-lg tracking-tight">Smart QR</span>
    </span>
  );
}

/** Standard content band — centered max-width container with consistent vertical rhythm. */
export function Section({
  id,
  children,
  muted = false,
  className = "",
}: {
  id?: string;
  children: ReactNode;
  muted?: boolean;
  className?: string;
}) {
  return (
    <section id={id} className={muted ? "bg-muted/40" : ""}>
      <div className={`mx-auto max-w-6xl px-6 py-16 sm:py-20 ${className}`}>{children}</div>
    </section>
  );
}

/** Eyebrow + title + description block that opens most sections. */
export function SectionHeading({
  eyebrow,
  title,
  description,
  align = "center",
}: {
  eyebrow?: string;
  title: string;
  description?: string;
  align?: "center" | "left";
}) {
  return (
    <div className={`max-w-2xl ${align === "center" ? "mx-auto text-center" : ""}`}>
      {eyebrow && <span className="text-sm font-semibold text-primary">{eyebrow}</span>}
      <h2 className="mt-2 text-3xl font-bold tracking-tight">{title}</h2>
      {description && <p className="mt-3 text-muted-foreground">{description}</p>}
    </div>
  );
}

export function FeatureCard({ feature }: { feature: Feature }) {
  const { icon: Icon, title, body } = feature;
  return (
    <div className="rounded-xl border border-border bg-card p-6">
      <span className="grid size-11 place-items-center rounded-lg bg-primary-soft text-primary">
        <Icon size={22} />
      </span>
      <h3 className="mt-4 text-lg font-semibold">{title}</h3>
      <p className="mt-2 text-sm leading-relaxed text-muted-foreground">{body}</p>
    </div>
  );
}

export function StepCard({ step, index }: { step: Step; index: number }) {
  const { icon: Icon, title, body } = step;
  return (
    <div className="relative overflow-hidden rounded-xl border border-border bg-card p-6">
      <span className="absolute right-4 top-3 text-5xl font-bold text-foreground/5">{index + 1}</span>
      <span className="grid size-11 place-items-center rounded-lg bg-primary-soft text-primary">
        <Icon size={22} />
      </span>
      <h3 className="mt-4 text-lg font-semibold">{title}</h3>
      <p className="mt-2 text-sm text-muted-foreground">{body}</p>
    </div>
  );
}

/** The four pricing tiers as a card grid. Every CTA drops the visitor into the guest builder. */
export function PricingCards({ tiers = PRICING }: { tiers?: PricingTier[] }) {
  return (
    <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-4">
      {tiers.map((tier) => (
        <PricingCard key={tier.id} tier={tier} />
      ))}
    </div>
  );
}

function PricingCard({ tier }: { tier: PricingTier }) {
  return (
    <div
      className={`relative flex flex-col rounded-2xl border bg-card p-6 ${
        tier.featured ? "border-primary shadow-lg shadow-primary/10" : "border-border"
      }`}
    >
      {tier.featured && (
        <span className="absolute -top-3 left-6 rounded-full bg-primary px-3 py-1 text-xs font-medium text-primary-foreground">
          Most popular
        </span>
      )}
      <h3 className="text-lg font-semibold">{tier.name}</h3>
      <div className="mt-2 flex items-baseline gap-1">
        <span className="text-3xl font-bold tracking-tight">{tier.price}</span>
        <span className="text-sm text-muted-foreground">{tier.cadence}</span>
      </div>
      <p className="mt-2 text-sm text-muted-foreground">{tier.tagline}</p>
      <ul className="mt-5 flex flex-1 flex-col gap-2.5">
        {tier.features.map((f) => (
          <li key={f} className="flex items-start gap-2 text-sm">
            <Check size={16} className="mt-0.5 shrink-0 text-primary" />
            <span>{f}</span>
          </li>
        ))}
      </ul>
      {tier.featured ? (
        <Button asChild className="mt-6" tone="primary" isFullWidth>
          <Link to="/app/new">{tier.cta}</Link>
        </Button>
      ) : (
        <Button asChild className="mt-6" tone="neutral" variant="outline" isFullWidth>
          <Link to="/app/new">{tier.cta}</Link>
        </Button>
      )}
    </div>
  );
}

/** Anti-incumbent comparison — the marketing's whole point in one table. */
export function ComparisonTable() {
  return (
    <div className="overflow-x-auto rounded-2xl border border-border">
      <table className="w-full min-w-[34rem] text-left text-sm">
        <thead>
          <tr className="border-b border-border bg-muted/50">
            <th className="px-5 py-4 font-medium text-muted-foreground"> </th>
            <th className="px-5 py-4 font-semibold text-primary">Smart QR</th>
            <th className="px-5 py-4 font-medium text-muted-foreground">Typical incumbent</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-border">
          {COMPARISON.map((row) => (
            <tr key={row.dimension}>
              <td className="px-5 py-4 font-medium">{row.dimension}</td>
              <td className="px-5 py-4">
                <span className="inline-flex items-start gap-2">
                  <Check size={16} className="mt-0.5 shrink-0 text-primary" />
                  <span>{row.smartQr}</span>
                </span>
              </td>
              <td className="px-5 py-4 text-muted-foreground">
                <span className="inline-flex items-start gap-2">
                  <X size={16} className="mt-0.5 shrink-0" />
                  <span>{row.incumbent}</span>
                </span>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

/** Lightweight FAQ accordion — one item open at a time. Hand-rolled to avoid lib-API surface. */
export function FaqList({ items }: { items: Faq[] }) {
  const [open, setOpen] = useState<number | null>(0);
  return (
    <div className="mx-auto max-w-2xl divide-y divide-border rounded-2xl border border-border">
      {items.map((item, i) => {
        const isOpen = open === i;
        return (
          <div key={item.q}>
            <button
              type="button"
              aria-expanded={isOpen}
              onClick={() => setOpen(isOpen ? null : i)}
              className="flex w-full items-center justify-between gap-4 px-5 py-4 text-left"
            >
              <span className="font-medium">{item.q}</span>
              <ChevronDown
                size={18}
                className={`shrink-0 text-muted-foreground transition-transform ${isOpen ? "rotate-180" : ""}`}
              />
            </button>
            {isOpen && <p className="-mt-1 px-5 pb-5 text-sm leading-relaxed text-muted-foreground">{item.a}</p>}
          </div>
        );
      })}
    </div>
  );
}

/** Closing call-to-action band. */
export function CtaBand() {
  return (
    <div className="mx-auto max-w-6xl px-6 pb-20">
      <div className="rounded-3xl border border-primary/20 bg-primary-soft/60 px-6 py-12 text-center sm:py-16">
        <h2 className="text-2xl font-bold tracking-tight sm:text-3xl">Your codes, forever.</h2>
        <p className="mx-auto mt-3 max-w-xl text-muted-foreground">
          Create a programmable code in under a minute. No account needed, unlimited scans, and it
          never expires on you.
        </p>
        <div className="mt-7 flex flex-wrap justify-center gap-3">
          <Button asChild tone="primary">
            <Link to="/app/new">Create your first code</Link>
          </Button>
          <Button asChild tone="neutral" variant="outline">
            <Link to="/pricing">See pricing</Link>
          </Button>
        </div>
      </div>
    </div>
  );
}

/** "One code, many destinations" — a live (client-side) QR beside the rules it resolves through. */
export function RoutingDemo() {
  const rules = [
    { icon: Smartphone, when: "On an iPhone", then: "→ App Store" },
    { icon: Globe, when: "In Germany", then: "→ German store" },
    { icon: Clock, when: "Before 4pm", then: "→ Lunch menu" },
    { icon: Languages, when: "Browser set to Russian", then: "→ RU landing page" },
  ];
  return (
    <div className="grid items-center gap-8 rounded-3xl border border-border bg-card p-8 lg:grid-cols-2">
      <div className="flex justify-center">
        <QrPreview value="https://smartqr.app/demo" foreground="#18181b" background="#ffffff" size={200} />
      </div>
      <div className="flex flex-col gap-3">
        <p className="text-sm font-medium text-muted-foreground">One printed code resolves by context:</p>
        {rules.map((rule) => {
          const Icon = rule.icon;
          return (
            <div
              key={rule.when}
              className="flex items-center gap-3 rounded-lg border border-border bg-background px-4 py-3"
            >
              <span className="grid size-9 shrink-0 place-items-center rounded-md bg-primary-soft text-primary">
                <Icon size={18} />
              </span>
              <span className="text-sm">
                <span className="font-medium">{rule.when}</span>{" "}
                <span className="text-muted-foreground">{rule.then}</span>
              </span>
            </div>
          );
        })}
        <p className="text-xs text-muted-foreground">
          …otherwise → your fallback URL. Change any of this without reprinting the code.
        </p>
      </div>
    </div>
  );
}

/** Decorative hero visual — a framed violet QR with a glow. Client-side preview, not the real asset. */
export function HeroVisual() {
  return (
    <div className="relative">
      <div className="absolute -inset-6 rounded-[2.5rem] bg-primary-soft/60 blur-2xl" aria-hidden />
      <div className="relative rounded-3xl border border-border bg-card p-6 shadow-xl shadow-primary/5">
        <QrPreview value="https://smartqr.app/menu" foreground="#6d28d9" background="#ffffff" size={220} />
        <div className="mt-4 flex items-center justify-between gap-2 text-xs">
          <span className="font-medium text-muted-foreground">smartqr.app/menu</span>
          <span className="rounded-full bg-success-soft px-2 py-0.5 font-medium text-success">routes 4 ways</span>
        </div>
      </div>
    </div>
  );
}

export function BlogCard({ post }: { post: PostMeta }) {
  return (
    <Link
      to={`/blog/${post.slug}`}
      className="group flex flex-col rounded-2xl border border-border bg-card p-6 transition-colors hover:border-primary/40"
    >
      <div className="flex items-center gap-2 text-xs text-muted-foreground">
        <span className="rounded-full bg-primary-soft px-2.5 py-1 font-medium text-primary">{post.tag}</span>
        <span>{post.readingMinutes} min read</span>
      </div>
      <h3 className="mt-4 text-lg font-semibold leading-snug group-hover:text-primary">{post.title}</h3>
      <p className="mt-2 flex-1 text-sm leading-relaxed text-muted-foreground">{post.description}</p>
      <span className="mt-4 inline-flex items-center gap-1 text-sm font-medium text-primary">
        Read <ArrowRight size={15} className="transition-transform group-hover:translate-x-0.5" />
      </span>
    </Link>
  );
}
