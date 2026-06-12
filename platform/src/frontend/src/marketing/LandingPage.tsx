import { Link } from "react-router-dom";
import { Button } from "@wow-two-beta/ui/actions";
import { ArrowRight, Infinity as InfinityIcon } from "lucide-react";
import { usePageMeta } from "../lib/meta";
import { FAQS, FEATURES, STEPS } from "./data";
import {
  ComparisonTable,
  CtaBand,
  FaqList,
  FeatureCard,
  HeroVisual,
  RoutingDemo,
  Section,
  SectionHeading,
  StepCard,
  PricingCards,
} from "./components";

export function LandingPage() {
  usePageMeta(
    "Smart QR — programmable codes that never expire",
    "One QR code, many destinations by context — and you'll never reprint it. Smart routing, every code type, unlimited scans, flat pricing. Your codes, forever.",
  );

  return (
    <>
      {/* ── Hero ── */}
      <section className="relative overflow-hidden">
        <div className="mx-auto grid max-w-6xl items-center gap-12 px-6 py-16 sm:py-24 lg:grid-cols-2">
          <div>
            <span className="inline-flex items-center gap-2 rounded-full border border-border bg-card px-3 py-1 text-xs text-muted-foreground">
              <InfinityIcon size={14} className="text-primary" />
              Codes never expire — on every plan
            </span>
            <h1 className="mt-5 text-4xl font-bold leading-[1.1] tracking-tight sm:text-5xl">
              A QR code smart enough to route every scan — and that you'll never reprint.
            </h1>
            <p className="mt-5 text-lg text-muted-foreground">
              Programmable routing, every code type, one flat price. Print it once; reprogram it
              forever; it never expires on you.
            </p>
            <div className="mt-8 flex flex-wrap gap-3">
              <Button asChild tone="primary">
                <Link to="/app/new">Get started free</Link>
              </Button>
              <Button asChild tone="neutral" variant="outline">
                <Link to="/pricing">See pricing</Link>
              </Button>
            </div>
            <p className="mt-4 text-xs text-muted-foreground">
              No account required · Unlimited scans · Free forever tier
            </p>
          </div>
          <div className="flex justify-center lg:justify-end">
            <HeroVisual />
          </div>
        </div>
      </section>

      {/* ── Features ── */}
      <Section id="features">
        <SectionHeading
          eyebrow="Features"
          title="Everything a printed code should do"
          description="The genuinely useful parts of a QR platform — unbundled from the up-sell and made the default."
        />
        <div className="mt-12 grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
          {FEATURES.map((feature) => (
            <FeatureCard key={feature.title} feature={feature} />
          ))}
        </div>
      </Section>

      {/* ── How it works ── */}
      <Section muted>
        <SectionHeading
          eyebrow="How it works"
          title="Print once. Reprogram forever."
          description="Three steps from idea to a code you'll never have to reprint."
        />
        <div className="mt-12 grid gap-5 md:grid-cols-3">
          {STEPS.map((step, i) => (
            <StepCard key={step.title} step={step} index={i} />
          ))}
        </div>
      </Section>

      {/* ── Routing demo ── */}
      <Section>
        <SectionHeading
          eyebrow="Smart routing"
          title="One code, many destinations"
          description="The headline feature — not a hidden premium toggle. Route every scan by who's scanning and when."
        />
        <div className="mt-12">
          <RoutingDemo />
        </div>
      </Section>

      {/* ── Pricing teaser ── */}
      <Section muted>
        <SectionHeading
          eyebrow="Pricing"
          title="Flat plans. Unlimited scans on all of them."
          description="Smart routing and a custom domain from $5 — not buried in a $30+ tier."
        />
        <div className="mt-12">
          <PricingCards />
        </div>
        <div className="mt-8 text-center">
          <Button asChild variant="ghost" tone="primary">
            <Link to="/pricing" className="inline-flex items-center gap-1">
              Compare plans in detail <ArrowRight size={15} />
            </Link>
          </Button>
        </div>
      </Section>

      {/* ── Comparison ── */}
      <Section>
        <SectionHeading
          title="Why we're different"
          description="The calm cut of a crowded category. Here's what changes when codes are treated as infrastructure, not hostages."
        />
        <div className="mt-12">
          <ComparisonTable />
        </div>
      </Section>

      {/* ── FAQ ── */}
      <Section muted>
        <SectionHeading title="Questions, answered" />
        <div className="mt-12">
          <FaqList items={FAQS} />
        </div>
      </Section>

      {/* ── Closing CTA ── */}
      <CtaBand />
    </>
  );
}
