import { Link } from "react-router-dom";
import { ColorTone, SizePreset } from "@wow-two-beta/ui/foundation/utils";
import { Button, ButtonVariant } from "@wow-two-beta/ui/presentation/actions";
import { Heading, HeadingSize, Text } from "@wow-two-beta/ui/presentation/display";
import { usePageMeta } from "@/presentation/common";
import { Section } from "./components";

export function NotFoundPage() {
  usePageMeta("Page not found · Smart QR");

  return (
    <Section>
      <div className="mx-auto max-w-md text-center">
        <Text as="span" size={SizePreset.Sm} weight="semibold" color="brand">
          404
        </Text>
        <Heading level={1} size={HeadingSize.Xxl} weight="bold" className="mt-2">
          This page wandered off
        </Heading>
        <Text color="muted" className="mt-3">
          The page you're looking for doesn't exist — but your codes are still safe, and still
          working.
        </Text>
        <div className="mt-7 flex flex-wrap justify-center gap-3">
          <Button asChild tone={ColorTone.Primary}>
            <Link to="/">Back home</Link>
          </Button>
          <Button asChild tone={ColorTone.Neutral} variant={ButtonVariant.Outline}>
            <Link to="/app">Open the app</Link>
          </Button>
        </div>
      </div>
    </Section>
  );
}
