import { Button } from "@wow-two-beta/ui/actions";
import { Select, TextInput } from "@wow-two-beta/ui/forms";
import { ArrowRight, ChevronDown, ChevronUp, Plus, Trash2 } from "lucide-react";
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

// Ordered conditional-rule editor — dense joined list, numbered by priority (first match wins,
// the rest falls through to the fallback URL). Reorder via the up/down handles (drag-reorder lands
// once the SDK `Sortable` ships in a published bump). Visual only — the data shape is unchanged.
export function RuleBuilder({ rules, onChange }: RuleBuilderProps) {
  const renumber = (list: RuleDraft[]) => list.map((r, i) => ({ ...r, order: i + 1 }));

  const update = (id: string, patch: Partial<RuleDraft>) =>
    onChange(rules.map((r) => (r.id === id ? { ...r, ...patch } : r)));

  const remove = (id: string) => onChange(renumber(rules.filter((r) => r.id !== id)));

  const move = (index: number, dir: -1 | 1) => {
    const to = index + dir;
    if (to < 0 || to >= rules.length) return;
    const next = [...rules];
    [next[index], next[to]] = [next[to], next[index]];
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

      {rules.map((rule, i) => (
        <div
          key={rule.id}
          className="flex items-start gap-2.5 border-b border-border p-3 last:border-b-0"
        >
          <div className="flex items-center gap-1 pt-1.5">
            <span className="grid size-5 shrink-0 place-items-center rounded-md bg-muted text-xs font-medium text-muted-foreground">
              {rule.order}
            </span>
            <div className="flex flex-col">
              <button
                type="button"
                aria-label="Move rule up"
                disabled={i === 0}
                onClick={() => move(i, -1)}
                className="text-subtle-foreground transition-colors hover:text-foreground disabled:cursor-not-allowed disabled:opacity-30"
              >
                <ChevronUp size={14} />
              </button>
              <button
                type="button"
                aria-label="Move rule down"
                disabled={i === rules.length - 1}
                onClick={() => move(i, 1)}
                className="text-subtle-foreground transition-colors hover:text-foreground disabled:cursor-not-allowed disabled:opacity-30"
              >
                <ChevronDown size={14} />
              </button>
            </div>
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
        </div>
      ))}

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
