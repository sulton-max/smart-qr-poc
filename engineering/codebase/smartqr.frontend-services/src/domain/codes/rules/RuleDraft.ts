import type { RuleConditionType } from "./RuleConditionType";

// Builder row; `id` is client-side for list keys.
export interface RuleDraft {
  id: string;
  order: number;
  conditionType: RuleConditionType;
  conditionValue: string;
  destination: string;
}
