/** Defines the content type a code carries — the discriminator on the wire's `CodeContent`. */
export const ContentType = {
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
  VCard: "vCard",

  /** Refers to a static calendar-event payload. */
  Calendar: "calendar",
} as const;

export type ContentType = (typeof ContentType)[keyof typeof ContentType];
