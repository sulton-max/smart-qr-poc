import { Text } from "@wow-two-beta/ui/display";
import { usePageMeta } from "../lib/meta";
import { PRICING_FAQS } from "./data";
import {
  ComparisonTable,
  CtaBand,
  FaqList,
  PricingCards,
  Section,
  SectionHeading,
} from "./components";

export function PricingPage() {
  usePageMeta(
    "Pricing — flat plans, unlimited scans · Smart QR",
    "Free, $5, $15, or $39 a month — flat. Unlimited scans on every tier, smart routing and a custom domain from $5, and codes that never expire. No scan caps, no surprise bills.",
  );

  return (
    <>
      <Section>
        <SectionHeading
          eyebrow="Pricing"
          title="Flat pricing. Unlimited scans. Codes that never expire."
          description="Every plan includes unlimited scans and the never-expire promise. Pay for more codes and routing power — never for the scans themselves."
        />
        <div className="mt-12">
          <PricingCards />
        </div>
        <Text size="sm" color="muted" align="center" className="mt-6">
          All plans: unlimited scans · codes never deactivate on downgrade · export &amp; delete
          anytime.
        </Text>
      </Section>

      <Section muted>
        <SectionHeading
          title="How we compare"
          description="The same three things people actually want — smart routing, a custom domain, and un-capped scans — without the $30+ paywall."
        />
        <div className="mt-12">
          <ComparisonTable />
        </div>
      </Section>

      <Section>
        <SectionHeading title="Pricing questions" />
        <div className="mt-12">
          <FaqList items={PRICING_FAQS} />
        </div>
      </Section>

      <CtaBand />
    </>
  );
}
