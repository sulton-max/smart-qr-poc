import type { CodeContent } from "../../content/models";
import type { CodeRuleType } from "../enums/CodeRuleType";
import type { RuleConditionType } from "../enums/RuleConditionType";

/** Represents a rule matched against a scan signal — evaluated in order, first match wins. */
export interface ConditionalRuleDto {
  /** The rule-role discriminator. */
  type: typeof CodeRuleType.Conditional;

  /** The 1-based evaluation order, and the rule's identity within its code. */
  order: number;

  /** The signal this rule matches on. */
  condition: RuleConditionType;

  /** The operand the condition compares against. */
  conditionValue: string;

  /** The content served when this rule matches. */
  content: CodeContent;
}
