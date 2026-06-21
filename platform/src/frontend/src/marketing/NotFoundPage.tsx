import { Link } from "react-router-dom";
import { Button } from "@wow-two-beta/ui/actions";
import { Heading, Text } from "@wow-two-beta/ui/display";
import { usePageMeta } from "../lib/meta";
import { Section } from "./components";

export function NotFoundPage() {
  usePageMeta("Page not found · Smart QR");

  return (
    <Section>
      <div className="mx-auto max-w-md text-center">
        <Text as="span" size="sm" weight="semibold" color="brand">
          404
        </Text>
        <Heading level={1} size="2xl" weight="bold" className="mt-2">
          This page wandered off
        </Heading>
        <Text color="muted" className="mt-3">
          The page you're looking for doesn't exist — but your codes are still safe, and still
          working.
        </Text>
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
