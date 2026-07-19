import type { ConditionalRuleDto } from "./ConditionalRuleDto";
import type { DefaultPointerRuleDto } from "./DefaultPointerRuleDto";
import type { DefaultRuleDto } from "./DefaultRuleDto";

/**
 * Represents one routing rule of a code. Conditional rules are matched in order, first match wins; at most one default
 * rule serves whatever the conditional rules did not — its absence means an unmatched scan does not resolve.
 */
export type CodeRuleDto = ConditionalRuleDto | DefaultPointerRuleDto | DefaultRuleDto;
