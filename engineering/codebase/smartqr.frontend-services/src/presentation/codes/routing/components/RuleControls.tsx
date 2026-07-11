import { ColorTone, SizePreset } from "@wow-two-beta/ui/foundation/utils";
import { Button, ButtonVariant } from "@wow-two-beta/ui/presentation/actions";
import { Field, Select, TextInput } from "@wow-two-beta/ui/presentation/forms";
import { Sortable } from "@wow-two-beta/ui/presentation/display";
import type { AppForm } from "@wow-two-beta/ui/forms-engine";
import { ArrowRight, GripVertical, Plus, Trash2 } from "lucide-react";
import { RuleConditionType, type RuleDraft } from "@/domain/codes/rules";
import { emptyRuleDraft, type CreateCodeValues } from "@/presentation/codes/common/createCode/CreateCodeForm";
import { RuleConditionTypeDisplays } from "./RuleConditionTypeDisplays";

/** Add-rule footer button label. */
const AddRuleLabel = "Add rule";

/** Defines props for the ordered routing-rule builder — reads the row snapshot, drives edits through `form.array`. */
export interface RuleControlsProps {
  /** The code-builder form (owns the `rules` array + the per-row field paths). */
  readonly form: AppForm<CreateCodeValues>;

  /** The current rule rows (first match wins) — the reactive snapshot from `form.Subscribe`. */
  readonly rules: ReadonlyArray<RuleDraft>;
}

/**
 * Renders an ordered conditional-rule editor — a dense joined list numbered by priority (first match wins,
 * the rest falls through to the fallback URL). Rows are driven by `form.array('rules')` (add / remove / reorder)
 * and each cell binds a `rules[i].*` field path; drag the handle to reorder (SDK `Sortable`). Row destinations
 * validate (and render errors) via the whole-form schema. Visual layout is unchanged.
 */
export function RuleControls({ form, rules }: RuleControlsProps) {
  const rulesArray = form.array("rules");

  const reorder = (from: number, to: number) => {
    const clamped = Math.max(0, Math.min(rules.length - 1, to));
    if (clamped !== from) rulesArray.move(from, clamped);
  };

  return (
    <div className="overflow-hidden rounded-xl border border-border bg-card">
      {rules.length === 0 && (
        <p className="px-4 py-3 text-sm text-muted-foreground">
          No rules yet — every scan goes to the fallback URL.
        </p>
      )}

      <Sortable onReorder={reorder}>
        {rules.map((rule, index) => (
          <Sortable.Item
            key={rule.id}
            index={index}
            className="flex items-start gap-2.5 border-b border-border p-3"
          >
            <div className="flex items-center gap-1.5 pt-1.5">
              <Sortable.Handle className="text-subtle-foreground hover:text-foreground">
                <GripVertical size={15} />
              </Sortable.Handle>
              <span className="grid size-5 shrink-0 place-items-center rounded-md bg-muted text-xs font-medium text-muted-foreground">
                {index + 1}
              </span>
            </div>

            <div className="grid flex-1 grid-cols-1 gap-2 sm:grid-cols-[minmax(0,9rem)_minmax(0,1fr)]">
              {/* Deep array-row paths resolve to `unknown` in the contract (typed values, loose deep paths) — cast per row. */}
              <form.Field name={`rules[${index}].conditionType`}>
                {(f) => (
                  <Select<RuleConditionType>
                    value={f.value as RuleConditionType}
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
              </form.Field>

              <form.Field name={`rules[${index}].conditionValue`}>
                {(f) => (
                  <TextInput
                    ring="sm"
                    value={f.value as string}
                    placeholder={RuleConditionTypeDisplays[rule.conditionType].placeholder}
                    onChange={(e) => f.setValue(e.target.value)}
                    onBlur={f.onBlur}
                  />
                )}
              </form.Field>

              <div className="flex items-center gap-2 sm:col-span-2">
                <ArrowRight size={14} className="shrink-0 text-subtle-foreground" />
                {/* Field chrome renders the row's schema error (e.g. required destination) on the right row. */}
                <form.Field name={`rules[${index}].destination`}>
                  {(f) => (
                    <Field className="flex-1">
                      <TextInput
                        ring="sm"
                        className="w-full"
                        value={f.value as string}
                        placeholder="https://destination-for-this-rule.com"
                        onChange={(e) => f.setValue(e.target.value)}
                        onBlur={f.onBlur}
                      />
                    </Field>
                  )}
                </form.Field>
              </div>
            </div>

            <Button
              tone={ColorTone.Danger}
              variant={ButtonVariant.Ghost}
              shape="square"
              aria-label="Remove rule"
              onClick={() => rulesArray.remove(index)}
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
        onClick={() => rulesArray.push(emptyRuleDraft())}
        className="justify-start rounded-none px-4 py-3 text-muted-foreground"
      >
        {AddRuleLabel}
      </Button>
    </div>
  );
}
