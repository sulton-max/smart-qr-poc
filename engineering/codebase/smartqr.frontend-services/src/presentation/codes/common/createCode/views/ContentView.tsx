import { Field, Select, TextInput } from "@wow-two-beta/ui/presentation/forms";
import { ContentTypeId, ContentTypes, isDynamicType, type FieldValues } from "@/domain/codes/content";
import type { CodeDto } from "@/domain/codes";
import { ContentTypeControls } from "@/presentation/codes/content/components/ContentTypeControls";

/** Defines props for the Content tab — the code's name, content type, and the per-type payload fields. */
export interface ContentViewProps {
  /** True in edit mode (surfaces the read-only short link for dynamic codes). */
  readonly isEdit: boolean;

  /** The loaded code in edit mode (its `shortUrl` backs the short-link field); null on create. */
  readonly existing: CodeDto | null;

  /** The code's display name. */
  readonly name: string;

  /** Fires when the name changes. */
  readonly onNameChange: (value: string) => void;

  /** The chosen content type (url / wifi / vCard / …). */
  readonly contentTypeId: ContentTypeId;

  /** Fires when the content type changes. */
  readonly onContentTypeIdChange: (value: ContentTypeId) => void;

  /** The dynamic forwarder's URL — the `url` type binds its single field here. */
  readonly fallbackUrl: string;

  /** Fires when the URL type's value changes. */
  readonly onFallbackUrlChange: (value: string) => void;

  /** The active content type's flat field values (every type except `url`). */
  readonly contentValues: FieldValues;

  /** Fires when a non-URL content type's field values change. */
  readonly onContentValuesChange: (values: FieldValues) => void;
}

/** Renders the Content tab: short link (edit + dynamic), name, content-type picker, and the per-type fields. */
export function ContentView({
  isEdit,
  existing,
  name,
  onNameChange,
  contentTypeId,
  onContentTypeIdChange,
  fallbackUrl,
  onFallbackUrlChange,
  contentValues,
  onContentValuesChange,
}: ContentViewProps) {
  return (
    <>
      {isEdit && existing && isDynamicType(contentTypeId) && (
        <Field label="Short link">
          <TextInput value={existing.shortUrl} readOnly disabled />
        </Field>
      )}
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
          value={contentTypeId}
          onValueChange={(o) => o && onContentTypeIdChange(o.itemKey)}
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
      <ContentTypeControls
        typeId={contentTypeId}
        values={contentTypeId === ContentTypeId.Url ? { url: fallbackUrl } : contentValues}
        onChange={(next) =>
          contentTypeId === ContentTypeId.Url ? onFallbackUrlChange(next.url ?? "") : onContentValuesChange(next)
        }
      />
    </>
  );
}
