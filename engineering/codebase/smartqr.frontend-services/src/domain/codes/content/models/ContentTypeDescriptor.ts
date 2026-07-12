import { ContentType } from "../ContentType";
import { ContentMode } from "../ContentMode";

/** Represents a selectable content type — its id, display label, and static/dynamic mode. */
export interface ContentTypeDescriptor {
  /** The content-type id (the `CodeContent` discriminator). */
  id: ContentType;

  /** The display label. */
  label: string;

  /** Whether the code bakes the payload (static) or carries the forwarder short link (dynamic). */
  mode: ContentMode;

  /** The optional helper text rendered above the fields. */
  note?: string;
}

/** The exhaustive content-type catalog — one descriptor per selectable type. */
export const contentTypeCatalog: ContentTypeDescriptor[] = [
  { id: ContentType.Url, label: "URL", mode: ContentMode.Dynamic },
  {
    id: ContentType.MobileApp,
    label: "Mobile app link",
    mode: ContentMode.Dynamic,
    note: "Add at least one. iPhone opens the App Store, Android opens Google Play; choose which link every other device opens.",
  },
  { id: ContentType.Text, label: "Text", mode: ContentMode.Static },
  { id: ContentType.Email, label: "Email", mode: ContentMode.Static },
  { id: ContentType.Sms, label: "SMS", mode: ContentMode.Static },
  { id: ContentType.Phone, label: "Phone", mode: ContentMode.Static },
  { id: ContentType.Geo, label: "Location", mode: ContentMode.Static },
  { id: ContentType.Wifi, label: "WiFi", mode: ContentMode.Static },
  { id: ContentType.VCard, label: "Contact card", mode: ContentMode.Static },
  { id: ContentType.Calendar, label: "Event", mode: ContentMode.Static },
];

const byId: Record<ContentType, ContentTypeDescriptor> = Object.fromEntries(
  contentTypeCatalog.map((c) => [c.id, c]),
) as Record<ContentType, ContentTypeDescriptor>;

/** Resolves a content-type descriptor by id. */
export const contentType = (id: ContentType): ContentTypeDescriptor => byId[id];
