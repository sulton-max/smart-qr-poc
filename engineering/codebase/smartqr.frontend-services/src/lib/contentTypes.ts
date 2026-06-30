// Static content types (v0.7) — the builder collects structured fields, and `encodeContent`
// turns them into the standard QR payload string that gets encoded into the code. `url` is the
// dynamic forwarder's destination; every other type bakes its payload directly (a static QR).

export type ContentTypeId =
  | "url"
  | "appstore"
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
  /** Key in the values record + the persisted field name. */
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
  fields: ContentField[];
  /** Build the QR payload string from collected field values. */
  encode: (v: FieldValues) => string;
}

const t = (v: string | undefined) => (v ?? "").trim();

/** Escape the WIFI: payload reserved characters (`\ ; , : "`). */
const escWifi = (v: string) => v.replace(/([\\;,:"])/g, "\\$1");

/** Escape a vCard / iCal property value (`\ ; ,` + newlines). */
const escIcal = (v: string) => v.replace(/([\\;,])/g, "\\$1").replace(/\r?\n/g, "\\n");

/** `2026-07-01T18:30` (datetime-local) → `20260701T183000`; date-only → `20260701`. */
export function toICalDate(s: string | undefined): string {
  const v = t(s);
  const dt = v.match(/^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2})(?::(\d{2}))?$/);
  if (dt) return `${dt[1]}${dt[2]}${dt[3]}T${dt[4]}${dt[5]}${dt[6] ?? "00"}`;
  const d = v.match(/^(\d{4})-(\d{2})-(\d{2})$/);
  return d ? `${d[1]}${d[2]}${d[3]}` : v;
}

export const CONTENT_TYPES: ContentTypeDef[] = [
  {
    id: "url",
    label: "URL",
    mode: "dynamic",
    fields: [{ key: "url", label: "Destination URL", kind: "url", placeholder: "https://example.com", required: true }],
    encode: (v) => t(v.url),
  },
  {
    // Dynamic + device-routed: the QR carries the forwarder short link; the redirect resolves the
    // scanner's OS (User-Agent) and sends iOS → App Store, Android → Google Play, else → fallback.
    // Mapped to the existing device/OS routing rules at save (see `appStoreRules`).
    id: "appstore",
    label: "App Store",
    mode: "dynamic",
    fields: [
      { key: "ios", label: "App Store (iOS) URL", kind: "url", placeholder: "https://apps.apple.com/app/…" },
      { key: "android", label: "Google Play URL", kind: "url", placeholder: "https://play.google.com/store/apps/…" },
      { key: "fallback", label: "Other devices URL", kind: "url", placeholder: "https://yourapp.com" },
    ],
    encode: (v) => t(v.fallback) || t(v.ios) || t(v.android),
  },
  {
    id: "text",
    label: "Text",
    mode: "static",
    fields: [{ key: "text", label: "Text", kind: "textarea", required: true }],
    encode: (v) => v.text ?? "",
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
    encode: (v) => {
      const params = new URLSearchParams();
      if (t(v.subject)) params.set("subject", t(v.subject));
      if (t(v.body)) params.set("body", t(v.body));
      const q = params.toString();
      return `mailto:${t(v.to)}${q ? `?${q}` : ""}`;
    },
  },
  {
    id: "sms",
    label: "SMS",
    mode: "static",
    fields: [
      { key: "phone", label: "Phone", kind: "tel", required: true },
      { key: "message", label: "Message", kind: "textarea" },
    ],
    encode: (v) => (t(v.message) ? `SMSTO:${t(v.phone)}:${t(v.message)}` : `SMSTO:${t(v.phone)}`),
  },
  {
    id: "phone",
    label: "Phone",
    mode: "static",
    fields: [{ key: "phone", label: "Phone", kind: "tel", placeholder: "+1 555 0100", required: true }],
    encode: (v) => `tel:${t(v.phone)}`,
  },
  {
    id: "geo",
    label: "Location",
    mode: "static",
    fields: [
      { key: "lat", label: "Latitude", kind: "number", required: true },
      { key: "lng", label: "Longitude", kind: "number", required: true },
    ],
    encode: (v) => `geo:${t(v.lat)},${t(v.lng)}`,
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
    encode: (v) => {
      const enc = t(v.encryption) || "WPA";
      const ssid = escWifi(v.ssid ?? "");
      const pass = enc === "nopass" ? "" : `P:${escWifi(v.password ?? "")};`;
      const hidden = v.hidden === "true" ? "H:true;" : "";
      return `WIFI:T:${enc};S:${ssid};${pass}${hidden};`;
    },
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
    encode: (v) => {
      const fn = [t(v.firstName), t(v.lastName)].filter(Boolean).join(" ");
      const lines = ["BEGIN:VCARD", "VERSION:3.0", `N:${escIcal(t(v.lastName))};${escIcal(t(v.firstName))};;;`, `FN:${escIcal(fn)}`];
      if (t(v.org)) lines.push(`ORG:${escIcal(t(v.org))}`);
      if (t(v.title)) lines.push(`TITLE:${escIcal(t(v.title))}`);
      if (t(v.phone)) lines.push(`TEL;TYPE=CELL:${escIcal(t(v.phone))}`);
      if (t(v.email)) lines.push(`EMAIL:${escIcal(t(v.email))}`);
      if (t(v.url)) lines.push(`URL:${escIcal(t(v.url))}`);
      if (t(v.address)) lines.push(`ADR:;;${escIcal(t(v.address))};;;;`);
      if (t(v.note)) lines.push(`NOTE:${escIcal(t(v.note))}`);
      lines.push("END:VCARD");
      return lines.join("\n");
    },
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
    encode: (v) => {
      const lines = ["BEGIN:VEVENT"];
      if (t(v.title)) lines.push(`SUMMARY:${escIcal(t(v.title))}`);
      if (t(v.start)) lines.push(`DTSTART:${toICalDate(v.start)}`);
      if (t(v.end)) lines.push(`DTEND:${toICalDate(v.end)}`);
      if (t(v.location)) lines.push(`LOCATION:${escIcal(t(v.location))}`);
      if (t(v.description)) lines.push(`DESCRIPTION:${escIcal(t(v.description))}`);
      lines.push("END:VEVENT");
      return lines.join("\n");
    },
  },
];

const BY_ID: Record<ContentTypeId, ContentTypeDef> = Object.fromEntries(
  CONTENT_TYPES.map((c) => [c.id, c]),
) as Record<ContentTypeId, ContentTypeDef>;

/** Look up a content-type definition by id. */
export const contentType = (id: ContentTypeId): ContentTypeDef => BY_ID[id];

/** Build the QR payload string for a content type from collected field values. */
export const encodeContent = (id: ContentTypeId, values: FieldValues): string => BY_ID[id].encode(values);

/** App Store device-routing → device-conditional destinations (iOS / Android); everything else uses the `fallback` field. Consumed when saving an `appstore` code into the existing device/OS routing rules. */
export function appStoreRules(v: FieldValues): { device: "iOS" | "Android"; url: string }[] {
  const rules: { device: "iOS" | "Android"; url: string }[] = [];
  if (t(v.ios)) rules.push({ device: "iOS", url: t(v.ios) });
  if (t(v.android)) rules.push({ device: "Android", url: t(v.android) });
  return rules;
}
