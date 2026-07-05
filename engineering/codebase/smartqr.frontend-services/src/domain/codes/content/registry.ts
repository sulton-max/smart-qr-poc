// Content-type registry (v0.7) — the per-type field definitions the builder collects. The BACKEND owns encoding:
// static types bake their payload from these fields server-side, dynamic types (url / mobileApp) resolve the
// redirect short link. No local payload encoders (that was the frontend↔backend drift risk this rewire removes).
// The value ↔ typed-`CodeContent` mapping ops live in `operations.ts`.

export type ContentTypeId =
  | "url"
  | "mobileApp"
  | "text"
  | "email"
  | "sms"
  | "phone"
  | "geo"
  | "wifi"
  | "vcard"
  | "calendar";

export type FieldKind = "text" | "url" | "tel" | "email" | "number" | "textarea" | "datetime" | "select";

export interface ContentField {
  /** Key in the values record + the typed content property name. */
  key: string;
  label: string;
  kind?: FieldKind;
  placeholder?: string;
  required?: boolean;
  /** Options for `kind: "select"` — value/label pairs. */
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

export const CONTENT_TYPES: ContentTypeDef[] = [
  {
    id: "url",
    label: "URL",
    mode: "dynamic",
    fields: [{ key: "url", label: "Destination URL", kind: "url", placeholder: "https://example.com", required: true }],
  },
  {
    // Dynamic + device-routed: the QR carries the forwarder short link; the redirect resolves the
    // scanner's OS (User-Agent) and sends iOS → App Store, Android → Google Play, else → the fallback.
    // The backend derives the device rules + fallback from these fields at save (see MobileAppLinkContentSpec).
    id: "mobileApp",
    label: "Mobile app link",
    mode: "dynamic",
    note: "Add at least one. iPhone opens the App Store, Android opens Google Play; choose which link every other device opens.",
    fields: [
      { key: "appStore", label: "App Store (iOS) URL", kind: "url", placeholder: "https://apps.apple.com/app/…" },
      { key: "playStore", label: "Google Play URL", kind: "url", placeholder: "https://play.google.com/store/apps/…" },
      { key: "other", label: "Other devices URL (fallback)", kind: "url", placeholder: "https://yourapp.com or another store" },
    ],
  },
  {
    id: "text",
    label: "Text",
    mode: "static",
    fields: [{ key: "text", label: "Text", kind: "textarea", required: true }],
  },
  {
    id: "email",
    label: "Email",
    mode: "static",
    fields: [
      { key: "to", label: "To", kind: "email", placeholder: "name@example.com", required: true },
      { key: "subject", label: "Subject" },
      { key: "body", label: "Body", kind: "textarea" },
    ],
  },
  {
    id: "sms",
    label: "SMS",
    mode: "static",
    fields: [
      { key: "phone", label: "Phone", kind: "tel", required: true },
      { key: "message", label: "Message", kind: "textarea" },
    ],
  },
  {
    id: "phone",
    label: "Phone",
    mode: "static",
    fields: [{ key: "phone", label: "Phone", kind: "tel", placeholder: "+1 555 0100", required: true }],
  },
  {
    id: "geo",
    label: "Location",
    mode: "static",
    fields: [
      { key: "latitude", label: "Latitude", kind: "number", required: true },
      { key: "longitude", label: "Longitude", kind: "number", required: true },
    ],
  },
  {
    id: "wifi",
    label: "WiFi",
    mode: "static",
    fields: [
      { key: "ssid", label: "Network name (SSID)", required: true },
      { key: "password", label: "Password", kind: "text" },
      {
        key: "encryption",
        label: "Security",
        kind: "select",
        options: [
          { value: "WPA", label: "WPA/WPA2" },
          { value: "WEP", label: "WEP" },
          { value: "nopass", label: "None" },
        ],
      },
      { key: "hidden", label: "Hidden network", kind: "select", options: [
        { value: "false", label: "No" },
        { value: "true", label: "Yes" },
      ] },
    ],
  },
  {
    id: "vcard",
    label: "Contact card",
    mode: "static",
    fields: [
      { key: "firstName", label: "First name", required: true },
      { key: "lastName", label: "Last name" },
      { key: "org", label: "Company" },
      { key: "title", label: "Title" },
      { key: "phone", label: "Phone", kind: "tel" },
      { key: "email", label: "Email", kind: "email" },
      { key: "url", label: "Website", kind: "url" },
      { key: "address", label: "Address" },
      { key: "note", label: "Note", kind: "textarea" },
    ],
  },
  {
    id: "calendar",
    label: "Event",
    mode: "static",
    fields: [
      { key: "title", label: "Title", required: true },
      { key: "start", label: "Starts", kind: "datetime", required: true },
      { key: "end", label: "Ends", kind: "datetime" },
      { key: "location", label: "Location" },
      { key: "description", label: "Description", kind: "textarea" },
    ],
  },
];

const BY_ID: Record<ContentTypeId, ContentTypeDef> = Object.fromEntries(
  CONTENT_TYPES.map((c) => [c.id, c]),
) as Record<ContentTypeId, ContentTypeDef>;

/** Look up a content-type definition by id. */
export const contentType = (id: ContentTypeId): ContentTypeDef => BY_ID[id];
