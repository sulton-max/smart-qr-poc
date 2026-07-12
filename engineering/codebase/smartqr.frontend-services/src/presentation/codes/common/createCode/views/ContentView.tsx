import { Field, Select, TextInput } from "@wow-two-beta/ui/presentation/forms";
import { Orientation } from "@wow-two-beta/ui/foundation/utils";
import { Divider } from "@wow-two-beta/ui/presentation/layout";
import type { AppForm } from "@wow-two-beta/ui/forms-engine";
import { ContentType, contentTypeCatalog, contentType, emptyContent, isDynamicType } from "@/domain/codes/content";
import type { CodeDto } from "@/domain/codes";
import type { CodeCreateUpdateApiRequest } from "@/integration/codes";
import { ContentTypeControls } from "@/presentation/codes/content/components/ContentTypeControls";

/** Defines props for the Content tab — the code's name and its typed content, bound to the builder form. */
export interface ContentViewProps {
  /** The code-builder form. */
  readonly form: AppForm<CodeCreateUpdateApiRequest>;

  /** True in edit mode (surfaces the read-only short link for dynamic codes). */
  readonly isEdit: boolean;

  /** The loaded code in edit mode (its `shortUrl` backs the short-link field); null on create. */
  readonly existingCode: CodeDto | null;
}

/** Renders the Content tab — static code identity (name + type) up top, then a rule, then the dynamic chosen content (short link + the type's typed fields). */
export function ContentView({ form, isEdit, existingCode }: ContentViewProps) {
  return (
    <>
      {/* Static — the code's identity: its name and the content type it carries. */}
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

      {/* `content` is the whole discriminated union bound as ONE field — `f.value` is the typed `CodeContent`,
          `f.setValue` takes it, so the type Select (reseeds via `emptyContent`) and every typed sub-control
          bind with no cast. */}
      <form.Field name="content">
        {(f) => (
          <>
            <Field label="Content type">
              <Select<ContentType>
                value={f.value.type}
                onValueChange={(o) => o && f.setValue(emptyContent(o.itemKey))}
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

            <Divider orientation={Orientation.Horizontal} />

            {/* Dynamic — the chosen content: the short link (edit + dynamic types) then the type's own fields. */}
            {isEdit && existingCode && isDynamicType(f.value.type) && (
              <Field label="Short link">
                <TextInput value={existingCode.shortUrl} readOnly disabled />
              </Field>
            )}
            <ContentTypeControls content={f.value} onChange={f.setValue} />
          </>
        )}
      </form.Field>
    </>
  );
}
