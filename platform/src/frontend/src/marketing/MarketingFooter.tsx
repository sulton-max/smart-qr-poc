import { Link } from "react-router-dom";
import { Heading, Text } from "@wow-two-beta/ui/display";
import { Container, Grid, VStack } from "@wow-two-beta/ui/layout";
import { Logo } from "./components";
import { BRAND } from "./data";

const PRODUCT_LINKS = [
  { to: "/pricing", label: "Pricing" },
  { to: "/app/new", label: "Create a code" },
  { to: "/app", label: "Open the app" },
];

const LEARN_LINKS = [
  { to: "/blog", label: "Blog" },
  { to: "/blog/why-qr-codes-should-never-expire", label: "Why codes never expire" },
  { to: "/blog/smart-routing-one-code-many-destinations", label: "How smart routing works" },
];

export function MarketingFooter() {
  const year = new Date().getFullYear();
  return (
    <footer className="border-t border-border bg-muted/30">
      <Container size="full" className="max-w-6xl px-6 py-12">
        <Grid columns="1" gap="10" className="sm:grid-cols-2 lg:grid-cols-4">
          <div className="lg:col-span-2">
            <Logo />
            <Text size="sm" color="muted" className="mt-3 max-w-xs">
              {BRAND.pitch}
            </Text>
            <Text size="xs" color="muted" className="mt-4">
              No hostage codes. No scan caps. No nags.
            </Text>
          </div>

          <FooterColumn title="Product" links={PRODUCT_LINKS} />
          <FooterColumn title="Learn" links={LEARN_LINKS} />
        </Grid>

        <div className="mt-10 flex flex-col items-start justify-between gap-2 border-t border-border pt-6 text-xs text-muted-foreground sm:flex-row sm:items-center">
          <span>© {year} Smart QR. Your codes, forever.</span>
          <span>Cancel, export, and delete everything — anytime.</span>
        </div>
      </Container>
    </footer>
  );
}

function FooterColumn({ title, links }: { title: string; links: { to: string; label: string }[] }) {
  return (
    <div>
      <Heading level={3} size="xs" weight="semibold" className="tracking-normal">
        {title}
      </Heading>
      <VStack as="ul" gap="2" className="mt-3">
        {links.map((link) => (
          <li key={link.to}>
            <Link to={link.to} className="text-sm text-muted-foreground transition-colors hover:text-foreground">
              {link.label}
            </Link>
          </li>
        ))}
      </VStack>
    </div>
  );
}
