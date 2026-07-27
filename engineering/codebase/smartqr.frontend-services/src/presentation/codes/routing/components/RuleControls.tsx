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
import { emptyConditionalRule, emptyDefaultRule } from "@/application/codes";
import { ContentTypeControls } from "../../content/components/ContentTypeControls";
import { RuleConditionTypeDisplays } from "./RuleConditionTypeDisplays";

/** Add-rule footer button label. */
const AddRuleLabel = "Add a routing rule";

/** Add-catch-all footer button label — only offered when the code has no catch-all. */
const AddDefaultLabel = "Add a catch-all";

/**
 * Renders the whole-set failures the server reports against `rules` — at most one catch-all, unique orders, a
 * live pointer target, non-empty. They belong to the list, not to any row, and `rules` is an array with no input
 * of its own, so without this they would be filed on a known path that nothing draws and vanish.
 */
function RuleSetErrors({ form }: { readonly form: AppForm<CodeCreateUpdateApiRequest> }) {
  return (
    <form.Field name="rules">
      {(f) =>
        f.errors.length === 0 ? null : (
          <ul className="flex flex-col gap-1">
            {f.errors.map((message) => (
              // Matches what the SDK `Field` renders for a leaf error, so a list error reads the same as a field one.
              <li key={message} className="text-sm text-destructive">
                {message}
              </li>
            ))}
          </ul>
        )
      }
    </form.Field>
  );
}

/** Defines props for the routing-rule builder — drives rule edits through `useFieldArray`. */
export interface RuleControlsProps {
  /** The code-builder form (owns the `rules` array + the per-row field paths). */
  readonly form: AppForm<CodeCreateUpdateApiRequest>;

  /** The content type every rule of this code carries. */
  readonly contentType: ContentType;
}

/**
 * Renders the content the code serves. One catch-all rule and nothing else — the common case — shows its content
 * bare. Add a rule and it becomes the rule list: conditionals matched in order (first match wins) then the
 * catch-all, each row carrying the content it serves. A scan matching no rule does not resolve.
 */
export function RuleControls({ form, contentType }: RuleControlsProps) {
  const rules = useFieldArray<CodeRuleDto>(form, "rules");

  // `FieldArray<TItem>` keys off `keyof TItem`, which collapses to the shared `type` on a union — so rows bind
  // through a second view typed as the widest variant. Same path, same state; only the field typing differs.
  const rows = useFieldArray<ConditionalRuleDto>(form, "rules");

  const values = form.useFormState((s) => s.values.rules);
  const defaultIndex = values.findIndex((rule) => rule.type !== CodeRuleType.Conditional);
  const isSingleDefault = values.length === 1 && values[0]?.type === CodeRuleType.Default;

  // A new conditional lands ahead of the catch-all — conditionals are matched in order, the catch-all serves the rest.
  const addRule = () => {
    const rule = emptyConditionalRule(contentType);
    if (defaultIndex < 0) rules.push(rule);
    else rules.insert(defaultIndex, rule);
  };

  const reorder = (from: number, to: number) => {
    const clamped = Math.max(0, Math.min(rules.length - 1, to));
    if (clamped !== from) rules.move(from, clamped);
  };

  // The plain code: no conditions to show, so no rule chrome either — just the content, plus the way out of it.
  if (isSingleDefault) {
    return (
      <div className="flex flex-col gap-4">
        <RuleSetErrors form={form} />

        <rows.Field index={0} name="content">
          {(f) => <ContentTypeControls content={f.value} onChange={(next) => f.setValue(next)} />}
        </rows.Field>

        <Button
          variant={ButtonVariant.Ghost}
          tone={ColorTone.Neutral}
          size={SizePreset.Sm}
          leadingSlot={<Plus size={16} />}
          onClick={addRule}
          className="-ml-2 self-start"
        >
          {AddRuleLabel}
        </Button>
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-2.5">
      <div className="flex flex-wrap items-baseline justify-between gap-x-3">
        <p className="text-sm font-medium">Routing rules</p>
        <p className="text-xs text-muted-foreground">Matched top-down — the first match wins</p>
      </div>

      <RuleSetErrors form={form} />

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
                              getOptionLabel={(id) => RuleConditionTypeDisplays[id].label}
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

        <div className="flex flex-wrap">
          <Button
            variant={ButtonVariant.Ghost}
            tone={ColorTone.Neutral}
            size={SizePreset.Sm}
            leadingSlot={<Plus size={16} />}
            onClick={addRule}
            className="flex-1 justify-start rounded-none px-4 py-3 text-muted-foreground"
          >
            {AddRuleLabel}
          </Button>

          {/* Without a catch-all an unmatched scan legitimately 404s — but removing one must not be one-way. */}
          {defaultIndex < 0 && (
            <Button
              variant={ButtonVariant.Ghost}
              tone={ColorTone.Neutral}
              size={SizePreset.Sm}
              leadingSlot={<Plus size={16} />}
              onClick={() => rules.push(emptyDefaultRule(contentType))}
              className="flex-1 justify-start rounded-none px-4 py-3 text-muted-foreground"
            >
              {AddDefaultLabel}
            </Button>
          )}
        </div>
      </div>
    </div>
  );
}
