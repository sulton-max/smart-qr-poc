import { Button } from "@wow-two-beta/ui/actions";
import { Select, TextInput } from "@wow-two-beta/ui/forms";
import { Plus, Trash2 } from "lucide-react";
import { RuleConditionType, type RuleDraft } from "../types";

export interface RuleBuilderProps {
  rules: RuleDraft[];
  onChange: (rules: RuleDraft[]) => void;
}

const CONDITION_LABEL: Record<RuleConditionType, string> = {
  Device: "Device",
  Country: "Country",
  Language: "Language",
  TimeOfDay: "Time of day",
};

const VALUE_PLACEHOLDER: Record<RuleConditionType, string> = {
  Device: "Ios · Android · Desktop",
  Country: "US",
  Language: "ru",
  TimeOfDay: "09:00-16:00",
};

/**
 * Domain component (in-project; not generic enough for the beta lib). Ordered conditional-rule
 * editor — composed from the lib's Select / TextInput / Button. First match wins; anything that
 * doesn't match falls through to the code's fallback URL.
 */
export function RuleBuilder({ rules, onChange }: RuleBuilderProps) {
  const update = (id: string, patch: Partial<RuleDraft>) =>
    onChange(rules.map((r) => (r.id === id ? { ...r, ...patch } : r)));

  const remove = (id: string) =>
    onChange(rules.filter((r) => r.id !== id).map((r, i) => ({ ...r, order: i + 1 })));

  const add = () =>
    onChange([
      ...rules,
      {
        id: crypto.randomUUID(),
        order: rules.length + 1,
        conditionType: RuleConditionType.Device,
        conditionValue: "",
        destination: "",
      },
    ]);

  return (
    <div className="flex flex-col gap-3">
      {rules.length === 0 && (
        <p className="text-sm text-muted-foreground">
          No rules yet — every scan goes to the fallback URL.
        </p>
      )}

      {rules.map((rule) => (
        <div
          key={rule.id}
          className="flex items-start gap-2 rounded-md border border-border bg-muted/30 p-3"
        >
          <span className="mt-2 w-4 shrink-0 text-xs text-muted-foreground">{rule.order}</span>

          <div className="grid flex-1 grid-cols-1 gap-2 sm:grid-cols-2">
            <Select<RuleConditionType>
              selected={rule.conditionType}
              onChange={(opt) => opt && update(rule.id, { conditionType: opt.itemKey })}
            >
              <Select.Trigger>
                <Select.Value />
              </Select.Trigger>
              <Select.Content>
                {Object.values(RuleConditionType).map((ct) => (
                  <Select.Item key={ct} itemKey={ct} label={CONDITION_LABEL[ct]} />
                ))}
              </Select.Content>
            </Select>

            <TextInput
              value={rule.conditionValue}
              placeholder={VALUE_PLACEHOLDER[rule.conditionType]}
              onChange={(e) => update(rule.id, { conditionValue: e.target.value })}
            />

            <TextInput
              className="sm:col-span-2"
              value={rule.destination}
              placeholder="https://destination-for-this-rule.com"
              onChange={(e) => update(rule.id, { destination: e.target.value })}
            />
          </div>

          <Button
            tone="danger"
            variant="ghost"
            shape="square"
            aria-label="Remove rule"
            onClick={() => remove(rule.id)}
          >
            <Trash2 size={16} />
          </Button>
        </div>
      ))}

      <Button variant="soft" tone="neutral" leadingSlot={<Plus size={16} />} onClick={add}>
        Add rule
      </Button>
    </div>
  );
}
