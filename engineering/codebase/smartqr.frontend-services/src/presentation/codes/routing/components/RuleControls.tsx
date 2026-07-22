import { ColorTone, SizePreset } from "@wow-two-beta/ui/foundation/utils";
import { Button, ButtonVariant } from "@wow-two-beta/ui/presentation/actions";
import { Select, TextInput } from "@wow-two-beta/ui/presentation/forms";
import { Sortable } from "@wow-two-beta/ui/presentation/display";
import { useFieldArray, type AppForm } from "@wow-two-beta/ui/forms-engine";
import { GripVertical, Plus, Trash2 } from "lucide-react";
import {
  CodeRuleType,
  RuleConditionType,
  type CodeRuleDto,
  type ConditionalRuleDto,
  type ContentType,
} from "@/domain/codes";
import type { CodeCreateUpdateApiRequest } from "@/integration/codes";
import { emptyConditionalRule } from "@/application/codes";
import { ContentTypeControls } from "../../content/components/ContentTypeControls";
import { RuleConditionTypeDisplays } from "./RuleConditionTypeDisplays";

/** Add-rule footer button label. */
const AddRuleLabel = "Add rule";

/** Defines props for the routing-rule builder — drives rule edits through `useFieldArray`. */
export interface RuleControlsProps {
  /** The code-builder form (owns the `rules` array + the per-row field paths). */
  readonly form: AppForm<CodeCreateUpdateApiRequest>;

  /** The content type every rule of this code carries. */
  readonly contentType: ContentType;
}

/**
 * Renders the routing-rule editor — conditional rows matched in order (first match wins), followed by the
 * optional catch-all. Each row carries the content it serves; a scan matching no rule does not resolve.
 */
export function RuleControls({ form, contentType }: RuleControlsProps) {
  const rules = useFieldArray<CodeRuleDto>(form, "rules");

  // `FieldArray<TItem>` keys off `keyof TItem`, which collapses to the shared `type` on a union — so rows bind
  // through a second view typed as the widest variant. Same path, same state; only the field typing differs.
  const rows = useFieldArray<ConditionalRuleDto>(form, "rules");

  const reorder = (from: number, to: number) => {
    const clamped = Math.max(0, Math.min(rules.length - 1, to));
    if (clamped !== from) rules.move(from, clamped);
  };

  return (
    <div className="overflow-hidden rounded-xl border border-border bg-card">
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

            {/* The row's shape follows its role, so the whole row re-renders when the role changes. */}
            <form.Subscribe selector={(s) => s.values.rules[row.index]}>
              {(rule) => (
                <div className="grid flex-1 grid-cols-1 gap-2">
                  {rule?.type === CodeRuleType.Conditional && (
                    <div className="grid grid-cols-1 gap-2 sm:grid-cols-[minmax(0,9rem)_minmax(0,1fr)]">
                      <rows.Field index={row.index} name="condition">
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
                      </rows.Field>

                      <rows.Field index={row.index} name="conditionValue">
                        {(f) => (
                          <TextInput
                            ring="sm"
                            value={f.value ?? ""}
                            placeholder={RuleConditionTypeDisplays[rule.condition].placeholder}
                            onChange={(e) => f.setValue(e.target.value)}
                            onBlur={f.onBlur}
                          />
                        )}
                      </rows.Field>
                    </div>
                  )}

                  {rule?.type === CodeRuleType.Default && (
                    <p className="text-sm text-muted-foreground">Everyone else</p>
                  )}

                  {rule && rule.type !== CodeRuleType.DefaultPointer && (
                    <rows.Field index={row.index} name="content">
                      {(f) => <ContentTypeControls content={f.value} onChange={(next) => f.setValue(next)} />}
                    </rows.Field>
                  )}
                </div>
              )}
            </form.Subscribe>

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
        onClick={() => rules.push(emptyConditionalRule(contentType))}
        className="justify-start rounded-none px-4 py-3 text-muted-foreground"
      >
        {AddRuleLabel}
      </Button>
    </div>
  );
}
