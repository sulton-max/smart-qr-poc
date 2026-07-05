// Typed content mirroring the backend polymorphic `CodeContent` — discriminated on `type`
// (the camelCase content id). The backend owns encoding: static types bake a payload from these
// fields, dynamic types (url / mobileApp) resolve the redirect short link. No `payload` on the wire.
// One named interface per member (mirrors the backend record names + field names exactly), then a
// union alias — so each content type has a nameable, referenceable shape.

export * from "./UrlContent";
export * from "./MobileAppLinkContent";
export * from "./TextContent";
export * from "./EmailContent";
export * from "./SmsContent";
export * from "./PhoneContent";
export * from "./GeoContent";
export * from "./WifiContent";
export * from "./VCardContent";
export * from "./CalendarContent";

import type { UrlContent } from "./UrlContent";
import type { MobileAppLinkContent } from "./MobileAppLinkContent";
import type { TextContent } from "./TextContent";
import type { EmailContent } from "./EmailContent";
import type { SmsContent } from "./SmsContent";
import type { PhoneContent } from "./PhoneContent";
import type { GeoContent } from "./GeoContent";
import type { WifiContent } from "./WifiContent";
import type { VCardContent } from "./VCardContent";
import type { CalendarContent } from "./CalendarContent";

/** The wire's polymorphic content — a discriminated union over `type` of every content shape. */
export type CodeContent =
  | UrlContent
  | MobileAppLinkContent
  | TextContent
  | EmailContent
  | SmsContent
  | PhoneContent
  | GeoContent
  | WifiContent
  | VCardContent
  | CalendarContent;
