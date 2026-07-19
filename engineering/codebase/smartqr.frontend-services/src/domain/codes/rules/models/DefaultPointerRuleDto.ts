import type { CodeRuleType } from "../enums/CodeRuleType";

/** Represents the catch-all delegating to another rule's content — nominates an existing rule rather than repeating it. */
export interface DefaultPointerRuleDto {
  /** The rule-role discriminator. */
  type: typeof CodeRuleType.DefaultPointer;

  /** The `order` of the rule whose content serves the unmatched scan. */
  targetOrder: number;
}
