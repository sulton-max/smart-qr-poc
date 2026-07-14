// Content operations — factories + predicates over the typed `CodeContent` union the builder holds directly.

import { contentType, type CodeContent } from "./models";
import { ContentType } from "./enums/ContentType";
import { ContentMode } from "./enums/ContentMode";

/** Builds the minimal typed content for a type — the discriminator plus its required fields blank. Used to seed a fresh content type. */
export function emptyContent(id: ContentType): CodeContent {
  switch (id) {
    case ContentType.Url:
      return { type: "url", url: "" };
    case ContentType.MobileApp:
      return { type: "mobileApp" };
    case ContentType.Text:
      return { type: "text", text: "" };
    case ContentType.Email:
      return { type: "email", to: "" };
    case ContentType.Sms:
      return { type: "sms", phone: "" };
    case ContentType.Phone:
      return { type: "phone", phone: "" };
    case ContentType.Geo:
      return { type: "geo", latitude: "", longitude: "" };
    case ContentType.Wifi:
      return { type: "wifi", ssid: "", hidden: false };
    case ContentType.VCard:
      return { type: "vCard", firstName: "" };
    case ContentType.Calendar:
      return { type: "calendar", title: "", start: "" };
  }
}

/** A code resolves through its redirect short link (dynamic) rather than a baked payload — true for url / mobileApp / absent content. */
export function isDynamicContent(content: CodeContent | null | undefined): boolean {
  return content == null || contentType(content.type).mode === ContentMode.Dynamic;
}

/** Whether a content type resolves through the forwarder short link (dynamic) rather than baking its payload (static). */
export function isDynamicType(id: ContentType): boolean {
  return contentType(id).mode === ContentMode.Dynamic;
}
