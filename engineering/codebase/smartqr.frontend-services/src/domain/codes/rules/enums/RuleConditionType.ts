/** Defines the signal a routing rule matches on (mirrors backend `conditionType`). */
export const RuleConditionType = {
  /** Refers to the catch-all — always matches, so it carries the code's destination when no other rule does. */
  Default: "default",
  /** Refers to the scanning device kind (iOS · Android · Desktop). */
  Device: "device",
  /** Refers to the visitor's country. */
  Country: "country",
  /** Refers to the visitor's preferred language. */
  Language: "language",
  /** Refers to the local time of day at scan. */
  TimeOfDay: "timeOfDay",
} as const;

export type RuleConditionType = (typeof RuleConditionType)[keyof typeof RuleConditionType];
