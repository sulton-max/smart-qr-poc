import { Button } from "@wow-two-beta/ui/actions";
import { Select, TextInput } from "@wow-two-beta/ui/forms";
import { Sortable } from "@wow-two-beta/ui/display";
import { ArrowRight, GripVertical, Plus, Trash2 } from "lucide-react";
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

// Ordered conditional-rule editor — dense joined list, numbered by priority (first match wins, the
// rest falls through to the fallback URL). Drag the handle to reorder (SDK `Sortable`). Visual only —
// the data shape is unchanged.
export function RuleBuilder({ rules, onChange }: RuleBuilderProps) {
  const renumber = (list: RuleDraft[]) => list.map((r, i) => ({ ...r, order: i + 1 }));

  const update = (id: string, patch: Partial<RuleDraft>) =>
    onChange(rules.map((r) => (r.id === id ? { ...r, ...patch } : r)));

  const remove = (id: string) => onChange(renumber(rules.filter((r) => r.id !== id)));

  const reorder = (from: number, to: number) => {
    const next = [...rules];
    const clamped = Math.max(0, Math.min(next.length - 1, to));
    const [moved] = next.splice(from, 1);
    next.splice(clamped, 0, moved);
    onChange(renumber(next));
  };

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
    <div className="overflow-hidden rounded-xl border border-border bg-card">
      {rules.length === 0 && (
        <p className="px-4 py-3 text-sm text-muted-foreground">
          No rules yet — every scan goes to the fallback URL.
        </p>
      )}

      <Sortable onReorder={reorder}>
        {rules.map((rule, i) => (
          <Sortable.Item
            key={rule.id}
            index={i}
            className="flex items-start gap-2.5 border-b border-border p-3"
          >
            <div className="flex items-center gap-1.5 pt-1.5">
              <Sortable.Handle className="text-subtle-foreground hover:text-foreground">
                <GripVertical size={15} />
              </Sortable.Handle>
              <span className="grid size-5 shrink-0 place-items-center rounded-md bg-muted text-xs font-medium text-muted-foreground">
                {rule.order}
              </span>
            </div>

            <div className="grid flex-1 grid-cols-1 gap-2 sm:grid-cols-[minmax(0,9rem)_minmax(0,1fr)]">
              <Select<RuleConditionType>
                value={rule.conditionType}
                onValueChange={(opt) => opt && update(rule.id, { conditionType: opt.itemKey })}
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

              <div className="flex items-center gap-2 sm:col-span-2">
                <ArrowRight size={14} className="shrink-0 text-subtle-foreground" />
                <TextInput
                  className="flex-1"
                  value={rule.destination}
                  placeholder="https://destination-for-this-rule.com"
                  onChange={(e) => update(rule.id, { destination: e.target.value })}
                />
              </div>
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
          </Sortable.Item>
        ))}
      </Sortable>

      <button
        type="button"
        onClick={add}
        className="flex w-full items-center gap-2 px-4 py-3 text-sm font-medium text-muted-foreground transition-colors hover:bg-muted hover:text-foreground"
      >
        <Plus size={16} />
        Add rule
      </button>
    </div>
  );
}
