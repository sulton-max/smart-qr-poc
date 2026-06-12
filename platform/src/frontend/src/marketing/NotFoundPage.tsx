import { Link } from "react-router-dom";
import { Button } from "@wow-two-beta/ui/actions";
import { usePageMeta } from "../lib/meta";
import { Section } from "./components";

export function NotFoundPage() {
  usePageMeta("Page not found · Smart QR");

  return (
    <Section>
      <div className="mx-auto max-w-md text-center">
        <span className="text-sm font-semibold text-primary">404</span>
        <h1 className="mt-2 text-3xl font-bold tracking-tight">This page wandered off</h1>
        <p className="mt-3 text-muted-foreground">
          The page you're looking for doesn't exist — but your codes are still safe, and still
          working.
        </p>
        <div className="mt-7 flex flex-wrap justify-center gap-3">
          <Button asChild tone="primary">
            <Link to="/">Back home</Link>
          </Button>
          <Button asChild tone="neutral" variant="outline">
            <Link to="/app">Open the app</Link>
          </Button>
        </div>
      </div>
    </Section>
  );
}
