import { contentType, type ContentTypeId, type FieldValues } from "@/domain/codes/content";
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

export interface ContentTypeFormProps {
  /** Which content type's fields to render. */
  readonly typeId: ContentTypeId;
  /** Current field values, keyed by `ContentField.key`. */
  readonly values: FieldValues;
  /** Emit the next values record. */
  readonly onChange: (next: FieldValues) => void;
}

// Each content type has a dedicated control group so its controls can diverge; this maps id → component.
const CONTROLS: Record<ContentTypeId, (props: ContentControlsProps) => React.JSX.Element> = {
  url: UrlControls,
  mobileApp: MobileAppControls,
  text: TextControls,
  email: EmailControls,
  sms: SmsControls,
  phone: PhoneControls,
  geo: GeoControls,
  wifi: WifiControls,
  vcard: VCardControls,
  calendar: CalendarControls,
};

/** Thin dispatcher — renders the chosen content type's dedicated control group (plus its optional note). */
export function ContentTypeForm({ typeId, values, onChange }: ContentTypeFormProps) {
  const def = contentType(typeId);
  const Controls = CONTROLS[typeId];

  return (
    <>
      {def.note && <p className="text-sm text-muted-foreground">{def.note}</p>}
      <Controls values={values} onChange={onChange} />
    </>
  );
}
