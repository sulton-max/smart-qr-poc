import { ColorTone, Orientation, SizePreset } from "@wow-two-beta/ui/foundation/utils";
import {
  Button,
  ButtonVariant,
  ToggleButton,
  ToggleButtonGroup,
  ToggleButtonGroupVariant,
  ToggleMode,
} from "@wow-two-beta/ui/presentation/actions";
import { Alert } from "@wow-two-beta/ui/presentation/feedback";
import { Divider } from "@wow-two-beta/ui/presentation/layout";
import { Field, Select, TextInput } from "@wow-two-beta/ui/presentation/forms";
import type { AppForm } from "@wow-two-beta/ui/forms-engine";
import {
  CodeRuleType,
  ContentMode,
  ContentType,
  contentType,
  contentTypeCatalog,
  emptyContent,
  type CodeDto,
  type CodeRuleDto,
} from "@/domain/codes";
import type { CodeCreateUpdateApiRequest } from "@/integration/codes";
import { ContentModeDisplays } from "@/presentation/codes/content/components/ContentModeDisplays";
import { RuleControls } from "@/presentation/codes/routing/components/RuleControls";

/** Defines props for the Content tab. */
export interface ContentViewProps {
  /** The code-builder form. */
  readonly form: AppForm<CodeCreateUpdateApiRequest>;

  /** Whether the builder is editing an existing code. */
  readonly isEdit: boolean;

  /** The loaded code being edited, when in edit mode. */
  readonly existingCode?: CodeDto;
}

/** Explains why the mode picker is locked — two destinations cannot be baked into one symbol. */
const ModeLockedNote = "More than one rule means the destination is decided at scan time — only a dynamic code can do that.";

/**
 * Renders the Content tab — the code's identity (name, content type, how it resolves) and the rules carrying
 * its content. Content and rules share one tab because a rule *is* where content lives.
 */
export function ContentView({ form, isEdit, existingCode }: ContentViewProps) {
  const rules = form.useFormState((s) => s.values.rules);
  const selectedMode = form.useFormState((s) => s.values.mode);
  const selectedContentType = form.useFormState((s) => s.values.contentType);

  // CM2: `rules.Count > 1` implies dynamic — so the picker locks rather than letting an unbakeable pair through.
  // The reverse doesn't hold: one rule stays a free choice. `toCreateCodeRequest` normalizes the sent value.
  const isModeLocked = rules.length > 1;
  const mode = isModeLocked ? ContentMode.Dynamic : selectedMode;

  return (
    <>
      <form.Field name="name">
        {(f) => (
          <Field label="Name">
            <TextInput
              ring="sm"
              value={f.value}
              placeholder="Spring menu table tent"
              onChange={(e) => f.setValue(e.target.value)}
              onBlur={f.onBlur}
            />
          </Field>
        )}
      </form.Field>

      {/* Switching the type reseeds every rule's content — a code carries one kind of content throughout. */}
      <form.Field name="contentType">
        {(f) => (
          <Field label="Content type">
            <Select<ContentType>
              value={f.value}
              onValueChange={(o) => {
                if (!o) return;
                f.setValue(o.itemKey);
                const reseeded = rules.map((rule: CodeRuleDto) =>
                  rule.type === CodeRuleType.DefaultPointer ? rule : { ...rule, content: emptyContent(o.itemKey) },
                );
                form.setValue("rules", reseeded);
              }}
              getOptionLabel={(id) => contentType(id).label}
            >
              <Select.Trigger>
                <Select.Value />
              </Select.Trigger>
              <Select.Content>
                {contentTypeCatalog.map((c) => (
                  <Select.Item key={c.id} itemKey={c.id} label={c.label} />
                ))}
              </Select.Content>
            </Select>
          </Field>
        )}
      </form.Field>

      {/* Mode is fixed at create: the two bake different bytes, so a printed code can never switch. */}
      {!isEdit && (
        <form.Field name="mode">
          {(f) => (
            <Field label="How it resolves" helper={isModeLocked ? ModeLockedNote : ContentModeDisplays[mode].note}>
              <ToggleButtonGroup<ContentMode>
                variant={ToggleButtonGroupVariant.Segmented}
                type={ToggleMode.Single}
                value={mode}
                onValueChange={(next) => next && f.setValue(next)}
                aria-label="How the code resolves"
              >
                {Object.values(ContentMode).map((option) => (
                  <ToggleButton
                    key={option}
                    value={option}
                    size={SizePreset.Sm}
                    isDisabled={isModeLocked && option !== ContentMode.Dynamic}
                  >
                    {ContentModeDisplays[option].label}
                  </ToggleButton>
                ))}
              </ToggleButtonGroup>
            </Field>
          )}
        </form.Field>
      )}

      {/* CM6 — the print decision is being made right here, so state the cost before it is irreversible. */}
      {!isEdit && mode === ContentMode.Static && (
        <Alert
          severity="info"
          title="The content is baked into the symbol"
          description="Anything already printed keeps this content forever — editing it later produces a different code. A dynamic code stays editable after printing."
          actions={
            <Button
              size={SizePreset.Sm}
              variant={ButtonVariant.Soft}
              tone={ColorTone.Primary}
              onClick={() => form.setValue("mode", ContentMode.Dynamic)}
            >
              Use dynamic
            </Button>
          }
        />
      )}

      {isEdit && existingCode?.shortUrl && (
        <Field label="Short link">
          <TextInput value={existingCode.shortUrl} readOnly disabled />
        </Field>
      )}

      <Divider orientation={Orientation.Horizontal} />

      <RuleControls form={form} contentType={selectedContentType} />
    </>
  );
}
