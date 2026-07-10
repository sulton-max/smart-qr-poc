import type { RuleDraft } from "@/domain/codes";
import { RuleControls } from "@/presentation/codes/routing/components/RuleControls";

/** Defines props for the Routing tab — the ordered redirect rules. */
export interface RoutingViewProps {
  /** The draft routing rules (client-side keyed). */
  readonly rules: RuleDraft[];

  /** Fires when the rule list changes. */
  readonly onRulesChange: (rules: RuleDraft[]) => void;
}

/** Renders the Routing tab: the rule builder that maps conditions to destinations. */
export function RoutingView({ rules, onRulesChange }: RoutingViewProps) {
  return <RuleControls rules={rules} onChange={onRulesChange} />;
}
