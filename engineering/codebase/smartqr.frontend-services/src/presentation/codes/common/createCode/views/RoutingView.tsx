import type { AppForm } from "@wow-two-beta/ui/forms-engine";
import type { CodeCreateUpdateApiRequest } from "@/integration/codes";
import { RuleControls } from "@/presentation/codes/routing/components/RuleControls";

/** Defines props for the Routing tab — the rules carrying the code's content, bound to the builder form. */
export interface RoutingViewProps {
  /** The code-builder form. */
  readonly form: AppForm<CodeCreateUpdateApiRequest>;
}

/** Renders the Routing tab: the rule builder, where each rule pairs a condition with the content it serves. */
export function RoutingView({ form }: RoutingViewProps) {
  return (
    <form.Subscribe selector={(s) => s.values.contentType}>
      {(contentType) => <RuleControls form={form} contentType={contentType} />}
    </form.Subscribe>
  );
}
