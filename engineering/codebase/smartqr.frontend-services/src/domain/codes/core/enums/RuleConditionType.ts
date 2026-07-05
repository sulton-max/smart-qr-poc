export const RuleConditionType = {
  Device: "device",
  Country: "country",
  Language: "language",
  TimeOfDay: "timeOfDay",
} as const;
export type RuleConditionType = (typeof RuleConditionType)[keyof typeof RuleConditionType];

/** Human-readable labels for RuleConditionType. */
export const RuleConditionTypeLabels: Record<RuleConditionType, string> = {
  [RuleConditionType.Device]: "Device",
  [RuleConditionType.Country]: "Country",
  [RuleConditionType.Language]: "Language",
  [RuleConditionType.TimeOfDay]: "Time of day",
};
