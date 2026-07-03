import { contentType, type ContentTypeId, type FieldValues } from "../lib/contentTypes";
import type { ContentControlsProps } from "./content/fields";
import { UrlControls } from "./content/UrlControls";
import { MobileAppControls } from "./content/MobileAppControls";
import { TextControls } from "./content/TextControls";
import { EmailControls } from "./content/EmailControls";
import { SmsControls } from "./content/SmsControls";
import { PhoneControls } from "./content/PhoneControls";
import { GeoControls } from "./content/GeoControls";
import { WifiControls } from "./content/WifiControls";
import { VCardControls } from "./content/VCardControls";
import { CalendarControls } from "./content/CalendarControls";

export interface ContentTypeFormProps {
  /** Which content type's fields to render. */
  typeId: ContentTypeId;
  /** Current field values, keyed by `ContentField.key`. */
  values: FieldValues;
  /** Emit the next values record. */
  onChange: (next: FieldValues) => void;
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
