import { ColorTone, SizePreset } from "@wow-two-beta/ui/foundation/utils";
import { Button, ButtonVariant } from "@wow-two-beta/ui/presentation/actions";
import { Select, TextInput } from "@wow-two-beta/ui/presentation/forms";
import { Sortable } from "@wow-two-beta/ui/presentation/display";
import { ArrowRight, GripVertical, Plus, Trash2 } from "lucide-react";
import { RuleConditionType, type RuleDraft } from "@/domain/codes/rules";
import { RuleConditionTypeDisplays } from "./RuleConditionTypeDisplays";

/** Add-rule footer button label. */
const AddRuleLabel = "Add rule";

/** Defines props for the ordered routing-rule builder. */
export interface RuleControlsProps {
  /** The ordered routing rules to edit, first match wins. */
  readonly rules: ReadonlyArray<RuleDraft>;

  /** Emits the next rule list on any add, edit, remove, or reorder. */
  readonly onChange: (rules: RuleDraft[]) => void;
}

/**
 * Renders an ordered conditional-rule editor — a dense joined list numbered by priority (first
 * match wins, the rest falls through to the fallback URL). Drag the handle to reorder (SDK
 * `Sortable`). Visual only — the data shape is unchanged.
 */
export function RuleControls({ rules, onChange }: RuleControlsProps) {
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
                    <Select.Item key={ct} itemKey={ct} label={RuleConditionTypeDisplays[ct].label} />
                  ))}
                </Select.Content>
              </Select>

              <TextInput
                ring="sm"
                value={rule.conditionValue}
                placeholder={RuleConditionTypeDisplays[rule.conditionType].placeholder}
                onChange={(e) => update(rule.id, { conditionValue: e.target.value })}
              />

              <div className="flex items-center gap-2 sm:col-span-2">
                <ArrowRight size={14} className="shrink-0 text-subtle-foreground" />
                <TextInput
                  ring="sm"
                  className="flex-1"
                  value={rule.destination}
                  placeholder="https://destination-for-this-rule.com"
                  onChange={(e) => update(rule.id, { destination: e.target.value })}
                />
              </div>
            </div>

            <Button
              tone={ColorTone.Danger}
              variant={ButtonVariant.Ghost}
              shape="square"
              aria-label="Remove rule"
              onClick={() => remove(rule.id)}
            >
              <Trash2 size={16} />
            </Button>
          </Sortable.Item>
        ))}
      </Sortable>

      <Button
        variant={ButtonVariant.Ghost}
        tone={ColorTone.Neutral}
        size={SizePreset.Sm}
        isFullWidth
        leadingSlot={<Plus size={16} />}
        onClick={add}
        className="justify-start rounded-none px-4 py-3 text-muted-foreground"
      >
        {AddRuleLabel}
      </Button>
    </div>
  );
}
