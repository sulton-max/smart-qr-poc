import { ColorTone, SizePreset } from "@wow-two-beta/ui/foundation/utils";
import { Button, ButtonVariant } from "@wow-two-beta/ui/presentation/actions";
import { Field, Select, TextInput } from "@wow-two-beta/ui/presentation/forms";
import { Sortable } from "@wow-two-beta/ui/presentation/display";
import { useFieldArray, type AppForm } from "@wow-two-beta/ui/forms-engine";
import { ArrowRight, GripVertical, Plus, Trash2 } from "lucide-react";
import { RuleConditionType, type CodeRuleDto } from "@/domain/codes/rules";
import type { CodeCreateUpdateApiRequest } from "@/integration/codes";
import { emptyCodeRule } from "@/application/codes";
import { RuleConditionTypeDisplays } from "./RuleConditionTypeDisplays";

/** Add-rule footer button label. */
const AddRuleLabel = "Add rule";

/** Defines props for the ordered routing-rule builder — drives rule edits through `useFieldArray`. */
export interface RuleControlsProps {
  /** The code-builder form (owns the `rules` array + the per-row field paths). */
  readonly form: AppForm<CodeCreateUpdateApiRequest>;
}

/**
 * Renders the ordered routing-rule editor — priority-numbered rows (first match wins, the rest fall through
 * to the fallback URL), driven by `useFieldArray('rules')` for add / remove / drag-reorder. Each cell binds a
 * typed row field; row destinations validate (and render errors) via the whole-form schema.
 */
export function RuleControls({ form }: RuleControlsProps) {
  const rules = useFieldArray<CodeRuleDto>(form, "rules");

  const reorder = (from: number, to: number) => {
    const clamped = Math.max(0, Math.min(rules.length - 1, to));
    if (clamped !== from) rules.move(from, clamped);
  };

  return (
    <div className="overflow-hidden rounded-xl border border-border bg-card">
      {rules.length === 0 && (
        <p className="px-4 py-3 text-sm text-muted-foreground">
          No rules yet — every scan goes to the fallback URL.
        </p>
      )}

      <Sortable onReorder={reorder}>
        {rules.rows.map((row) => (
          <Sortable.Item
            key={row.key}
            index={row.index}
            className="flex items-start gap-2.5 border-b border-border p-3"
          >
            <div className="flex items-center gap-1.5 pt-1.5">
              <Sortable.Handle className="text-subtle-foreground hover:text-foreground">
                <GripVertical size={15} />
              </Sortable.Handle>
              <span className="grid size-5 shrink-0 place-items-center rounded-md bg-muted text-xs font-medium text-muted-foreground">
                {row.index + 1}
              </span>
            </div>

            <div className="grid flex-1 grid-cols-1 gap-2 sm:grid-cols-[minmax(0,9rem)_minmax(0,1fr)]">
              <rules.Field index={row.index} name="conditionType">
                {(f) => (
                  <Select<RuleConditionType>
                    value={f.value}
                    onValueChange={(opt) => opt && f.setValue(opt.itemKey)}
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
                )}
              </rules.Field>

              {/* Placeholder tracks this row's condition type. */}
              <form.Subscribe
                selector={(s) => s.values.rules[row.index]?.conditionType ?? RuleConditionType.Device}
              >
                {(conditionType) => (
                  <rules.Field index={row.index} name="conditionValue">
                    {(f) => (
                      <TextInput
                        ring="sm"
                        value={f.value ?? ""}
                        placeholder={RuleConditionTypeDisplays[conditionType].placeholder}
                        onChange={(e) => f.setValue(e.target.value)}
                        onBlur={f.onBlur}
                      />
                    )}
                  </rules.Field>
                )}
              </form.Subscribe>

              <div className="flex items-center gap-2 sm:col-span-2">
                <ArrowRight size={14} className="shrink-0 text-subtle-foreground" />
                {/* Field chrome renders the row's schema error on the right row. */}
                <rules.Field index={row.index} name="destination">
                  {(f) => (
                    <Field className="flex-1">
                      <TextInput
                        ring="sm"
                        className="w-full"
                        value={f.value}
                        placeholder="https://destination-for-this-rule.com"
                        onChange={(e) => f.setValue(e.target.value)}
                        onBlur={f.onBlur}
                      />
                    </Field>
                  )}
                </rules.Field>
              </div>
            </div>

            <Button
              tone={ColorTone.Danger}
              variant={ButtonVariant.Ghost}
              shape="square"
              aria-label="Remove rule"
              onClick={() => rules.remove(row.index)}
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
        onClick={() => rules.push(emptyCodeRule())}
        className="justify-start rounded-none px-4 py-3 text-muted-foreground"
      >
        {AddRuleLabel}
      </Button>
    </div>
  );
}
