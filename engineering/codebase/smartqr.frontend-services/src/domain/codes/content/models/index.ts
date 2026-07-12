// codes/content models — one interface per content variant + the discriminated `CodeContent` union.
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

export * from "./ContentTypeDescriptor";

/** Represents the wire's polymorphic content — a discriminated union over `type` of every content shape. */
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
