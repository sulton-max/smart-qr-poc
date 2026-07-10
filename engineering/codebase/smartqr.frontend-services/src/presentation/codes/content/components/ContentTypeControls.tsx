import { ContentTypeId, contentType, type CodeContent } from "@/domain/codes/content";
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

/** Defines props for the content dispatcher — the typed content model + a change handler. */
export interface ContentTypeControlsProps {
  /** The current typed content (discriminated on `type`). */
  readonly content: CodeContent;

  /** Emits the next typed content. */
  readonly onChange: (next: CodeContent) => void;
}

/** Renders the chosen content type's dedicated typed control group, plus its optional note. */
export function ContentTypeControls({ content, onChange }: ContentTypeControlsProps) {
  const def = contentType(content.type);

  return (
    <>
      {def.note && <p className="text-sm text-muted-foreground">{def.note}</p>}
      {renderControls(content, onChange)}
    </>
  );
}

// Each content type has a dedicated typed control so its controls can diverge; the switch narrows the union to
// the matching model. `onChange` (over the full union) is passed as-is — a wider handler satisfies a narrower one.
function renderControls(content: CodeContent, onChange: (next: CodeContent) => void) {
  switch (content.type) {
    case ContentTypeId.Url:
      return <UrlControls value={content} onChange={onChange} />;
    case ContentTypeId.MobileApp:
      return <MobileAppControls value={content} onChange={onChange} />;
    case ContentTypeId.Text:
      return <TextControls value={content} onChange={onChange} />;
    case ContentTypeId.Email:
      return <EmailControls value={content} onChange={onChange} />;
    case ContentTypeId.Sms:
      return <SmsControls value={content} onChange={onChange} />;
    case ContentTypeId.Phone:
      return <PhoneControls value={content} onChange={onChange} />;
    case ContentTypeId.Geo:
      return <GeoControls value={content} onChange={onChange} />;
    case ContentTypeId.Wifi:
      return <WifiControls value={content} onChange={onChange} />;
    case ContentTypeId.VCard:
      return <VCardControls value={content} onChange={onChange} />;
    case ContentTypeId.Calendar:
      return <CalendarControls value={content} onChange={onChange} />;
    default: {
      // Exhaustiveness guard — a new content type must add a control above.
      const _exhaustive: never = content;
      return _exhaustive;
    }
  }
}
