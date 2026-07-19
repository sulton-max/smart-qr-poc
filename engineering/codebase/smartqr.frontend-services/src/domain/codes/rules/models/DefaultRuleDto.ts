import type { CodeContent } from "../../content/models";
import type { CodeRuleType } from "../enums/CodeRuleType";

/** Represents the catch-all serving its own content — carries no order or condition, since it is never matched. */
export interface DefaultRuleDto {
  /** The rule-role discriminator. */
  type: typeof CodeRuleType.Default;

  /** The content served when no conditional rule matches. */
  content: CodeContent;
}
