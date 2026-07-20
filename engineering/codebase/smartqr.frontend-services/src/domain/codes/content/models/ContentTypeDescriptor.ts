import { ContentType } from "../enums/ContentType";

/** Represents a selectable content type — its id and display label. */
export interface ContentTypeDescriptor {
  /** The content-type id (the `CodeContent` discriminator). */
  id: ContentType;

  /** The display label. */
  label: string;

  /** The optional helper text rendered above the fields. */
  note?: string;
}

/** The exhaustive content-type catalog — one descriptor per selectable type. */
export const contentTypeCatalog: ContentTypeDescriptor[] = [
  { id: ContentType.Url, label: "URL" },
  {
    id: ContentType.MobileApp,
    label: "Mobile app link",
    note: "Add at least one. iPhone opens the App Store, Android opens Google Play; choose which link every other device opens.",
  },
  { id: ContentType.Text, label: "Text" },
  { id: ContentType.Email, label: "Email" },
  { id: ContentType.Sms, label: "SMS" },
  { id: ContentType.Phone, label: "Phone" },
  { id: ContentType.Geo, label: "Location" },
  { id: ContentType.Wifi, label: "WiFi" },
  { id: ContentType.VCard, label: "Contact card" },
  { id: ContentType.Calendar, label: "Event" },
];

const byId: Record<ContentType, ContentTypeDescriptor> = Object.fromEntries(
  contentTypeCatalog.map((c) => [c.id, c]),
) as Record<ContentType, ContentTypeDescriptor>;

/** Resolves a content-type descriptor by id. */
export const contentType = (id: ContentType): ContentTypeDescriptor => byId[id];
