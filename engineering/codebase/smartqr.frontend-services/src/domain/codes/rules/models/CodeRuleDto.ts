import type { RuleConditionType } from "../RuleConditionType";

/** Represents a single routing rule — a condition mapped to a destination, matched in order. */
export interface CodeRuleDto {
  /** The 1-based match order (first match wins). */
  order: number;

  /** The signal this rule matches on. */
  conditionType: RuleConditionType;

  /** The value the condition compares against. */
  conditionValue?: string;

  /** The destination URL used when this rule matches. */
  destination: string;
}
