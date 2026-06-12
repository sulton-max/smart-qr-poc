import { Link } from "react-router-dom";
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
      <div className="mx-auto max-w-6xl px-6 py-12">
        <div className="grid gap-10 sm:grid-cols-2 lg:grid-cols-4">
          <div className="lg:col-span-2">
            <Logo />
            <p className="mt-3 max-w-xs text-sm text-muted-foreground">{BRAND.pitch}</p>
            <p className="mt-4 text-xs text-muted-foreground">
              No hostage codes. No scan caps. No nags.
            </p>
          </div>

          <FooterColumn title="Product" links={PRODUCT_LINKS} />
          <FooterColumn title="Learn" links={LEARN_LINKS} />
        </div>

        <div className="mt-10 flex flex-col items-start justify-between gap-2 border-t border-border pt-6 text-xs text-muted-foreground sm:flex-row sm:items-center">
          <span>© {year} Smart QR. Your codes, forever.</span>
          <span>Cancel, export, and delete everything — anytime.</span>
        </div>
      </div>
    </footer>
  );
}

function FooterColumn({ title, links }: { title: string; links: { to: string; label: string }[] }) {
  return (
    <div>
      <h3 className="text-sm font-semibold">{title}</h3>
      <ul className="mt-3 flex flex-col gap-2">
        {links.map((link) => (
          <li key={link.to}>
            <Link to={link.to} className="text-sm text-muted-foreground transition-colors hover:text-foreground">
              {link.label}
            </Link>
          </li>
        ))}
      </ul>
    </div>
  );
}
