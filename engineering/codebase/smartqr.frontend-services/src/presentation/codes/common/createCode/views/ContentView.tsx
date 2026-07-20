import { SizePreset } from "@wow-two-beta/ui/foundation/utils";
import {
  ToggleButton,
  ToggleButtonGroup,
  ToggleButtonGroupVariant,
  ToggleMode,
} from "@wow-two-beta/ui/presentation/actions";
import { Divider, Orientation } from "@wow-two-beta/ui/presentation/layout";
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

/** Defines props for the Content tab. */
export interface ContentViewProps {
  /** The code-builder form. */
  readonly form: AppForm<CodeCreateUpdateApiRequest>;

  /** Whether the builder is editing an existing code. */
  readonly isEdit: boolean;

  /** The loaded code being edited, when in edit mode. */
  readonly existingCode?: CodeDto;
}

/** The mode picker's labels — the choice is permanent, so the copy says what each costs. */
const ModeDisplays: Record<ContentMode, { label: string; note: string }> = {
  [ContentMode.Static]: {
    label: "Static",
    note: "The code carries the content itself — it works offline, and changing the content makes a different code.",
  },
  [ContentMode.Dynamic]: {
    label: "Dynamic",
    note: "The code carries a short link — edit the content any time without reprinting.",
  },
};

/** Renders the Content tab — the code's identity: its name, the kind of content it carries, and how it resolves. */
export function ContentView({ form, isEdit, existingCode }: ContentViewProps) {
  const rules = form.useFormState((s) => s.values.rules);

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
            <Field label="How it resolves" helper={ModeDisplays[f.value ?? ContentMode.Static].note}>
              <ToggleButtonGroup<ContentMode>
                variant={ToggleButtonGroupVariant.Segmented}
                type={ToggleMode.Single}
                value={f.value ?? ContentMode.Static}
                onValueChange={(mode) => mode && f.setValue(mode)}
                aria-label="How the code resolves"
              >
                {Object.values(ContentMode).map((mode) => (
                  <ToggleButton key={mode} value={mode} size={SizePreset.Sm}>
                    {ModeDisplays[mode].label}
                  </ToggleButton>
                ))}
              </ToggleButtonGroup>
            </Field>
          )}
        </form.Field>
      )}

      <Divider orientation={Orientation.Horizontal} />

      {isEdit && existingCode?.shortUrl && (
        <Field label="Short link">
          <TextInput value={existingCode.shortUrl} readOnly disabled />
        </Field>
      )}

      <p className="text-sm text-muted-foreground">
        Enter the content on the Routing tab — each rule carries the content it serves.
      </p>
    </>
  );
}
