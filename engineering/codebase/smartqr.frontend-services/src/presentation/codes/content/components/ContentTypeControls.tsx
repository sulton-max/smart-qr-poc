import { ContentTypeId, contentType, type FieldValues } from "@/domain/codes/content";
import type { ContentControlsProps } from "./fields";
import { UrlControls } from "./UrlControls";
import { MobileAppControls } from "./MobileAppControls";
import { TextControls } from "./TextControls";
import { EmailControls } from "./EmailControls";
import { SmsControls } from "./SmsControls";
import { PhoneControls } from "./PhoneControls";
import { GeoControls } from "./GeoControls";
import { WifiControls } from "./WifiControls";
import { VCardControls } from "./VCardControls";
import { CalendarControls } from "./CalendarControls";

/** Defines props for the content-type dispatcher. */
export interface ContentTypeControlsProps {
  /** The content type whose fields to render. */
  readonly typeId: ContentTypeId;

  /** The current field values, keyed by `ContentField.key`. */
  readonly values: FieldValues;

  /** Emits the next values record. */
  readonly onChange: (next: FieldValues) => void;
}

// Each content type has a dedicated control group so its controls can diverge; this maps id → component.
const Controls: Record<ContentTypeId, (props: ContentControlsProps) => React.JSX.Element> = {
  [ContentTypeId.Url]: UrlControls,
  [ContentTypeId.MobileApp]: MobileAppControls,
  [ContentTypeId.Text]: TextControls,
  [ContentTypeId.Email]: EmailControls,
  [ContentTypeId.Sms]: SmsControls,
  [ContentTypeId.Phone]: PhoneControls,
  [ContentTypeId.Geo]: GeoControls,
  [ContentTypeId.Wifi]: WifiControls,
  [ContentTypeId.VCard]: VCardControls,
  [ContentTypeId.Calendar]: CalendarControls,
};

/** Renders the chosen content type's dedicated control group, plus its optional note. */
export function ContentTypeControls({ typeId, values, onChange }: ContentTypeControlsProps) {
  const def = contentType(typeId);
  const TypeControls = Controls[typeId];

  return (
    <>
      {def.note && <p className="text-sm text-muted-foreground">{def.note}</p>}
      <TypeControls fieldValues={values} onChange={onChange} />
    </>
  );
}
