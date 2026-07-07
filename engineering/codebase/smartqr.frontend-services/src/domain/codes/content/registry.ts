// Content-type registry (v0.7) — the per-type field definitions the builder collects. The BACKEND owns encoding:
// static types bake their payload from these fields server-side, dynamic types (url / mobileApp) resolve the
// redirect short link. No local payload encoders (that was the frontend↔backend drift risk this rewire removes).
// The value ↔ typed-`CodeContent` mapping ops live in `operations.ts`.

/** Defines the content type a code carries — the discriminator on the wire's `CodeContent`. */
export const ContentTypeId = {
  /** Refers to a dynamic forwarder to a single destination URL. */
  Url: "url",

  /** Refers to a device-routed link — App Store / Google Play / other-device fallback. */
  MobileApp: "mobileApp",

  /** Refers to a static free-text payload. */
  Text: "text",

  /** Refers to a static mailto payload — recipient, subject, body. */
  Email: "email",

  /** Refers to a static SMS payload — recipient plus a prefilled message. */
  Sms: "sms",

  /** Refers to a static tel payload — a dial number. */
  Phone: "phone",

  /** Refers to a static geo payload — a latitude / longitude pair. */
  Geo: "geo",

  /** Refers to a static WiFi-join payload — SSID, password, security, hidden flag. */
  Wifi: "wifi",

  /** Refers to a static contact-card (vCard) payload. */
  VCard: "vcard",

  /** Refers to a static calendar-event payload. */
  Calendar: "calendar",
} as const;

export type ContentTypeId = (typeof ContentTypeId)[keyof typeof ContentTypeId];

/** Defines the input primitive a content field binds to. */
export const FieldKind = {
  /** Refers to a single-line text input. */
  Text: "text",

  /** Refers to a single-line URL input. */
  Url: "url",

  /** Refers to a single-line telephone input. */
  Tel: "tel",

  /** Refers to a single-line email input. */
  Email: "email",

  /** Refers to a single-line numeric input. */
  Number: "number",

  /** Refers to a multi-line text input. */
  TextArea: "textarea",

  /** Refers to a `datetime-local` input. */
  DateTime: "datetime",

  /** Refers to a select over value/label options. */
  Select: "select",
} as const;

export type FieldKind = (typeof FieldKind)[keyof typeof FieldKind];

export interface ContentField {
  /** Key in the values record + the typed content property name. */
  key: string;

  label: string;

  kind?: FieldKind;

  placeholder?: string;

  required?: boolean;

  /** Options for `kind: FieldKind.Select` — value/label pairs. */
  options?: { value: string; label: string }[];
}

export type FieldValues = Record<string, string | undefined>;

export interface ContentTypeDef {
  id: ContentTypeId;

  label: string;

  /** Whether the QR carries the payload directly (static) or the forwarder short link (dynamic). */
  mode: "static" | "dynamic";

  /** Optional helper text rendered above the fields (e.g. "add at least one"). */
  note?: string;

  fields: ContentField[];
}

export const ContentTypes: ContentTypeDef[] = [
  {
    id: ContentTypeId.Url,
    label: "URL",
    mode: "dynamic",
    fields: [{ key: "url", label: "Destination URL", kind: FieldKind.Url, placeholder: "https://example.com", required: true }],
  },
  {
    // Dynamic + device-routed: the QR carries the forwarder short link; the redirect resolves the
    // scanner's OS (User-Agent) and sends iOS → App Store, Android → Google Play, else → the fallback.
    // The backend derives the device rules + fallback from these fields at save (see MobileAppLinkContentSpec).
    id: ContentTypeId.MobileApp,
    label: "Mobile app link",
    mode: "dynamic",
    note: "Add at least one. iPhone opens the App Store, Android opens Google Play; choose which link every other device opens.",
    fields: [
      { key: "appStore", label: "App Store (iOS) URL", kind: FieldKind.Url, placeholder: "https://apps.apple.com/app/…" },
      { key: "playStore", label: "Google Play URL", kind: FieldKind.Url, placeholder: "https://play.google.com/store/apps/…" },
      { key: "other", label: "Other devices URL (fallback)", kind: FieldKind.Url, placeholder: "https://yourapp.com or another store" },
    ],
  },
  {
    id: ContentTypeId.Text,
    label: "Text",
    mode: "static",
    fields: [{ key: "text", label: "Text", kind: FieldKind.TextArea, required: true }],
  },
  {
    id: ContentTypeId.Email,
    label: "Email",
    mode: "static",
    fields: [
      { key: "to", label: "To", kind: FieldKind.Email, placeholder: "name@example.com", required: true },
      { key: "subject", label: "Subject" },
      { key: "body", label: "Body", kind: FieldKind.TextArea },
    ],
  },
  {
    id: ContentTypeId.Sms,
    label: "SMS",
    mode: "static",
    fields: [
      { key: "phone", label: "Phone", kind: FieldKind.Tel, required: true },
      { key: "message", label: "Message", kind: FieldKind.TextArea },
    ],
  },
  {
    id: ContentTypeId.Phone,
    label: "Phone",
    mode: "static",
    fields: [{ key: "phone", label: "Phone", kind: FieldKind.Tel, placeholder: "+1 555 0100", required: true }],
  },
  {
    id: ContentTypeId.Geo,
    label: "Location",
    mode: "static",
    fields: [
      { key: "latitude", label: "Latitude", kind: FieldKind.Number, required: true },
      { key: "longitude", label: "Longitude", kind: FieldKind.Number, required: true },
    ],
  },
  {
    id: ContentTypeId.Wifi,
    label: "WiFi",
    mode: "static",
    fields: [
      { key: "ssid", label: "Network name (SSID)", required: true },
      { key: "password", label: "Password", kind: FieldKind.Text },
      {
        key: "encryption",
        label: "Security",
        kind: FieldKind.Select,
        options: [
          { value: "WPA", label: "WPA/WPA2" },
          { value: "WEP", label: "WEP" },
          { value: "nopass", label: "None" },
        ],
      },
      { key: "hidden", label: "Hidden network", kind: FieldKind.Select, options: [
        { value: "false", label: "No" },
        { value: "true", label: "Yes" },
      ] },
    ],
  },
  {
    id: ContentTypeId.VCard,
    label: "Contact card",
    mode: "static",
    fields: [
      { key: "firstName", label: "First name", required: true },
      { key: "lastName", label: "Last name" },
      { key: "org", label: "Company" },
      { key: "title", label: "Title" },
      { key: "phone", label: "Phone", kind: FieldKind.Tel },
      { key: "email", label: "Email", kind: FieldKind.Email },
      { key: "url", label: "Website", kind: FieldKind.Url },
      { key: "address", label: "Address" },
      { key: "note", label: "Note", kind: FieldKind.TextArea },
    ],
  },
  {
    id: ContentTypeId.Calendar,
    label: "Event",
    mode: "static",
    fields: [
      { key: "title", label: "Title", required: true },
      { key: "start", label: "Starts", kind: FieldKind.DateTime, required: true },
      { key: "end", label: "Ends", kind: FieldKind.DateTime },
      { key: "location", label: "Location" },
      { key: "description", label: "Description", kind: FieldKind.TextArea },
    ],
  },
];

const ById: Record<ContentTypeId, ContentTypeDef> = Object.fromEntries(
  ContentTypes.map((c) => [c.id, c]),
) as Record<ContentTypeId, ContentTypeDef>;

/** Look up a content-type definition by id. */
export const contentType = (id: ContentTypeId): ContentTypeDef => ById[id];
