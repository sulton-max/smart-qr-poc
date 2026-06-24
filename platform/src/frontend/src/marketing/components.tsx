import { type ReactNode } from "react";
import { Link } from "react-router-dom";
import { Button } from "@wow-two-beta/ui/actions";
import { Accordion, Badge, Card, FeatureCard as UiFeatureCard, Heading, PricingCard as UiPricingCard, StepCard as UiStepCard, Table, TableBody, TableCell, TableHead, TableHeaderCell, TableRow, Text } from "@wow-two-beta/ui/display";
import { Container, Grid, HStack, Section as UiSection, Surface, VStack } from "@wow-two-beta/ui/layout";
import {
  ArrowRight,
  Check,
  Clock,
  Globe,
  Languages,
  QrCode,
  Smartphone,
  X,
} from "lucide-react";
import { QRCodeSVG } from "qrcode.react";
import { COMPARISON, type Faq, type Feature, PRICING, type PricingTier, type Step } from "./data";
import { type PostMeta } from "./blog/types";

/**
 * Static decorative QR for marketing pages — pure client-side render (no backend
 * call). The builder's live preview uses the server-rendered `QrPreview` instead;
 * these landing visuals are illustrative and must work without the API.
 */
function StaticQrPreview({
  value,
  foreground,
  background,
  size = 200,
}: {
  value: string;
  foreground: string;
  background: string;
  size?: number;
}) {
  return (
    <div
      className="inline-flex items-center justify-center rounded-xl p-4"
      style={{ backgroundColor: background }}
    >
      <QRCodeSVG
        value={value || " "}
        size={size}
        level="Q"
        fgColor={foreground}
        bgColor={background}
        marginSize={2}
      />
    </div>
  );
}

/** Brand mark — shared by marketing header/footer and the app shell. */
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

/** Content band; inner `Container` owns the `max-w-6xl` width (SDK size scale maps to breakpoints, not 6xl). `muted` overrides SDK neutral tint to match original look. */
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
    <UiSection
      id={id}
      bleed
      py="none"
      tone={muted ? "neutral" : undefined}
      className={muted ? "border-transparent bg-muted/40" : undefined}
    >
      <Container size="full" className={`max-w-6xl px-6 py-16 sm:py-20 ${className}`}>
        {children}
      </Container>
    </UiSection>
  );
}

/** Eyebrow, title, description block. */
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
      {eyebrow && (
        <Text as="span" size="sm" weight="semibold" color="brand">
          {eyebrow}
        </Text>
      )}
      <Heading level={2} size="2xl" weight="bold" className="mt-2">
        {title}
      </Heading>
      {description && (
        <Text color="muted" className="mt-3">
          {description}
        </Text>
      )}
    </div>
  );
}

export function FeatureCard({ feature }: { feature: Feature }) {
  const { icon: Icon, title, body } = feature;
  return <UiFeatureCard icon={<Icon size={22} />} title={title} description={body} />;
}

export function StepCard({ step, index }: { step: Step; index: number }) {
  const { icon: Icon, title, body } = step;
  return <UiStepCard step={index + 1} icon={<Icon size={22} />} title={title} description={body} />;
}

/** Pricing tier grid. Marketing stays backend-independent — no API calls; paid CTAs defer Checkout to the in-app billing screen. */
export function PricingCards({ tiers = PRICING }: { tiers?: PricingTier[] }) {
  return (
    <Grid columns="1" gap="5" className="sm:grid-cols-2 lg:grid-cols-4">
      {tiers.map((tier) => (
        <PricingCard key={tier.id} tier={tier} />
      ))}
    </Grid>
  );
}

/** Free → guest builder; paid → in-app billing screen. */
function tierCtaHref(tier: PricingTier): string {
  return tier.id === "free" ? "/app/new" : "/app/billing";
}

function PricingCard({ tier }: { tier: PricingTier }) {
  const href = tierCtaHref(tier);
  return (
    <UiPricingCard
      name={tier.name}
      price={tier.price}
      cadence={tier.cadence}
      tagline={tier.tagline}
      features={tier.features}
      featured={tier.featured}
    >
      {tier.featured ? (
        <Button asChild tone="primary" isFullWidth>
          <Link to={href}>{tier.cta}</Link>
        </Button>
      ) : (
        <Button asChild tone="neutral" variant="outline" isFullWidth>
          <Link to={href}>{tier.cta}</Link>
        </Button>
      )}
    </UiPricingCard>
  );
}

/** Anti-incumbent comparison table. `headVariant="plain"` keeps normal-case heads; className overrides per-column accents. */
export function ComparisonTable() {
  return (
    <Table radius="2xl" density="roomy" className="min-w-[34rem]">
      <TableHead headVariant="plain">
        <TableRow>
          <TableHeaderCell className="font-medium text-muted-foreground"> </TableHeaderCell>
          <TableHeaderCell className="text-primary">Smart QR</TableHeaderCell>
          <TableHeaderCell className="font-medium text-muted-foreground">Typical incumbent</TableHeaderCell>
        </TableRow>
      </TableHead>
      <TableBody>
        {COMPARISON.map((row) => (
          <TableRow key={row.dimension}>
            <TableCell className="font-medium">{row.dimension}</TableCell>
            <TableCell>
              <span className="inline-flex items-start gap-2">
                <Check size={16} className="mt-0.5 shrink-0 text-primary" />
                <span>{row.smartQr}</span>
              </span>
            </TableCell>
            <TableCell className="text-muted-foreground">
              <span className="inline-flex items-start gap-2">
                <X size={16} className="mt-0.5 shrink-0" />
                <span>{row.incumbent}</span>
              </span>
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}

/** FAQ accordion — single open, first open by default. */
export function FaqList({ items }: { items: Faq[] }) {
  return (
    <Accordion
      type="single"
      isCollapsible
      defaultValue="0"
      className="mx-auto max-w-2xl divide-y divide-border overflow-hidden rounded-2xl border border-border [&>*]:border-b-0"
    >
      {items.map((item, i) => (
        <Accordion.Item key={item.q} value={String(i)} className="border-b-0">
          <Accordion.Trigger className="gap-4 px-5 py-4 text-base font-medium hover:bg-transparent [&>svg]:size-[18px]">
            {item.q}
          </Accordion.Trigger>
          <Accordion.Content className="-mt-1 px-5 pb-5 leading-relaxed text-muted-foreground">
            {item.a}
          </Accordion.Content>
        </Accordion.Item>
      ))}
    </Accordion>
  );
}

/** Closing call-to-action band. */
export function CtaBand() {
  return (
    <Container size="full" className="max-w-6xl px-6 pb-20">
      <Surface
        variant="subtle"
        tone="primary"
        className="rounded-3xl px-6 py-12 text-center sm:py-16"
      >
        <Heading level={2} size="xl" weight="bold" className="sm:text-3xl">
          Your codes, forever.
        </Heading>
        <Text color="muted" className="mx-auto mt-3 max-w-xl">
          Create a programmable code in under a minute. No account needed, unlimited scans, and it
          never expires on you.
        </Text>
        <div className="mt-7 flex flex-wrap justify-center gap-3">
          <Button asChild tone="primary">
            <Link to="/app/new">Create your first code</Link>
          </Button>
          <Button asChild tone="neutral" variant="outline">
            <Link to="/pricing">See pricing</Link>
          </Button>
        </div>
      </Surface>
    </Container>
  );
}

/** Client-side QR beside the rules it resolves through. */
export function RoutingDemo() {
  const rules = [
    { icon: Smartphone, when: "On an iPhone", then: "→ App Store" },
    { icon: Globe, when: "In Germany", then: "→ German store" },
    { icon: Clock, when: "Before 4pm", then: "→ Lunch menu" },
    { icon: Languages, when: "Browser set to Russian", then: "→ RU landing page" },
  ];
  return (
    <Card
      variant="outline"
      elevation={0}
      className="grid items-center gap-8 rounded-3xl bg-card p-8 lg:grid-cols-2"
    >
      <div className="flex justify-center">
        <StaticQrPreview value="https://smartqr.app/demo" foreground="#18181b" background="#ffffff" size={200} />
      </div>
      <VStack gap="3">
        <Text size="sm" weight="medium" color="muted">
          One printed code resolves by context:
        </Text>
        {rules.map((rule) => {
          const Icon = rule.icon;
          return (
            <HStack
              key={rule.when}
              align="center"
              gap="3"
              className="rounded-lg border border-border bg-background px-4 py-3"
            >
              <span className="grid size-9 shrink-0 place-items-center rounded-md bg-primary-soft text-primary">
                <Icon size={18} />
              </span>
              <span className="text-sm">
                <span className="font-medium">{rule.when}</span>{" "}
                <span className="text-muted-foreground">{rule.then}</span>
              </span>
            </HStack>
          );
        })}
        <Text size="xs" color="muted">
          …otherwise → your fallback URL. Change any of this without reprinting the code.
        </Text>
      </VStack>
    </Card>
  );
}

/** Hero visual — client-side preview, not the real asset. */
export function HeroVisual() {
  return (
    <div className="relative">
      <div className="absolute -inset-6 rounded-[2.5rem] bg-primary-soft/60 blur-2xl" aria-hidden />
      <div className="relative rounded-3xl border border-border bg-card p-6 shadow-xl shadow-primary/5">
        <StaticQrPreview value="https://smartqr.app/menu" foreground="#6d28d9" background="#ffffff" size={220} />
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
    <Surface
      asChild
      variant="outline"
      radius="2xl"
      className="group flex flex-col bg-card p-6 transition-colors hover:border-primary/40"
    >
      <Link to={`/blog/${post.slug}`}>
        <div className="flex items-center gap-2 text-xs text-muted-foreground">
          <Badge variant="brand" size="md" className="px-2.5 py-1 text-primary">
            {post.tag}
          </Badge>
          <span>{post.readingMinutes} min read</span>
        </div>
        <Heading level={3} size="md" className="mt-4 leading-snug tracking-normal group-hover:text-primary">
          {post.title}
        </Heading>
        <Text size="sm" color="muted" className="mt-2 flex-1 leading-relaxed">
          {post.description}
        </Text>
        <span className="mt-4 inline-flex items-center gap-1 text-sm font-medium text-primary">
          Read <ArrowRight size={15} className="transition-transform group-hover:translate-x-0.5" />
        </span>
      </Link>
    </Surface>
  );
}
