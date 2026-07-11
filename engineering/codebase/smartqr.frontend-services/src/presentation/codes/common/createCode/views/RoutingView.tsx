import type { AppForm } from "@wow-two-beta/ui/forms-engine";
import { RuleControls } from "@/presentation/codes/routing/components/RuleControls";
import type { CreateCodeValues } from "../CreateCodeForm";

/** Defines props for the Routing tab — the ordered redirect rules, bound to the builder form. */
export interface RoutingViewProps {
  /** The code-builder form. */
  readonly form: AppForm<CreateCodeValues>;
}

/** Renders the Routing tab: the rule builder that maps conditions to destinations, driven by `form.array('rules')`. */
export function RoutingView({ form }: RoutingViewProps) {
  return (
    <form.Subscribe selector={(s) => s.values.rules}>
      {(rules) => <RuleControls form={form} rules={rules} />}
    </form.Subscribe>
  );
}
