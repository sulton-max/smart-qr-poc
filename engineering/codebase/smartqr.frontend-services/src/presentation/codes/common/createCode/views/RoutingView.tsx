import type { AppForm } from "@wow-two-beta/ui/forms-engine";
import type { CodeCreateUpdateApiRequest } from "@/integration/codes";
import { RuleControls } from "@/presentation/codes/routing/components/RuleControls";

/** Defines props for the Routing tab — the ordered redirect rules, bound to the builder form. */
export interface RoutingViewProps {
  /** The code-builder form. */
  readonly form: AppForm<CodeCreateUpdateApiRequest>;
}

/** Renders the Routing tab: the rule builder that maps conditions to destinations, driven by `useFieldArray('rules')`. */
export function RoutingView({ form }: RoutingViewProps) {
  return <RuleControls form={form} />;
}
