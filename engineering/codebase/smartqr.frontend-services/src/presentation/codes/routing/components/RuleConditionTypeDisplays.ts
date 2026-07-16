import { RuleConditionType } from "@/domain/codes/rules";

/** Defines the display for a routing-condition option. */
interface RuleConditionTypeDisplay {
  label: string;
  placeholder: string;
}

/** Maps each routing condition to its label + value-input placeholder. */
export const RuleConditionTypeDisplays: Record<RuleConditionType, RuleConditionTypeDisplay> = {
  [RuleConditionType.Default]: { label: "Everyone else", placeholder: "" },
  [RuleConditionType.Device]: { label: "Device", placeholder: "Ios · Android · Desktop" },
  [RuleConditionType.Country]: { label: "Country", placeholder: "US" },
  [RuleConditionType.Language]: { label: "Language", placeholder: "ru" },
  [RuleConditionType.TimeOfDay]: { label: "Time of day", placeholder: "09:00-16:00" },
};
