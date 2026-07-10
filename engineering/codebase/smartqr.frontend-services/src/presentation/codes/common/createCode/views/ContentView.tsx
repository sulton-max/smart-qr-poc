import { Field, Select, TextInput } from "@wow-two-beta/ui/presentation/forms";
import { Orientation } from "@wow-two-beta/ui/foundation/utils";
import { Divider } from "@wow-two-beta/ui/presentation/layout";
import { ContentTypeId, ContentTypes, contentType, emptyContent, isDynamicType, type CodeContent } from "@/domain/codes/content";
import type { CodeDto } from "@/domain/codes";
import { ContentTypeControls } from "@/presentation/codes/content/components/ContentTypeControls";

/** Defines props for the Content tab — the code's name and its typed content. */
export interface ContentViewProps {
  /** True in edit mode (surfaces the read-only short link for dynamic codes). */
  readonly isEdit: boolean;

  /** The loaded code in edit mode (its `shortUrl` backs the short-link field); null on create. */
  readonly existingCode: CodeDto | null;

  /** The code's display name. */
  readonly name: string;

  /** Fires when the name changes. */
  readonly onNameChange: (value: string) => void;

  /** The typed content the code carries (discriminated on `type`). */
  readonly content: CodeContent;

  /** Fires when the content (type or fields) changes. */
  readonly onContentChange: (content: CodeContent) => void;
}

/** Renders the Content tab — static code identity (name + type) up top, then a rule, then the dynamic chosen content (short link + the type's typed fields). */
export function ContentView({ isEdit, existingCode, name, onNameChange, content, onContentChange }: ContentViewProps) {
  const typeId = content.type;
  return (
    <>
      {/* Static — the code's identity: its name and the content type it carries. */}
      <Field label="Name">
        <TextInput
          ring="sm"
          value={name}
          placeholder="Spring menu table tent"
          onChange={(e) => onNameChange(e.target.value)}
        />
      </Field>
      <Field label="Content type">
        <Select<ContentTypeId>
          value={typeId}
          onValueChange={(o) => o && onContentChange(emptyContent(o.itemKey))}
          getOptionLabel={(id) => contentType(id).label}
        >
          <Select.Trigger>
            <Select.Value />
          </Select.Trigger>
          <Select.Content>
            {ContentTypes.map((c) => (
              <Select.Item key={c.id} itemKey={c.id} label={c.label} />
            ))}
          </Select.Content>
        </Select>
      </Field>

      <Divider orientation={Orientation.Horizontal} />

      {/* Dynamic — the chosen content: the short link (edit + dynamic types) then the type's own fields. */}
      {isEdit && existingCode && isDynamicType(typeId) && (
        <Field label="Short link">
          <TextInput value={existingCode.shortUrl} readOnly disabled />
        </Field>
      )}
      <ContentTypeControls content={content} onChange={onContentChange} />
    </>
  );
}
