// Content operations — factories + predicates over the typed `CodeContent` union the builder holds directly.
// Payload ENCODING lives on the backend (static types bake their payload; dynamic types resolve the redirect
// short link). Each content type's fields are edited via its typed control (`UrlControls`, `WifiControls`, …);
// there's no flat form-values bag — the builder's content state *is* the wire shape.

import type { CodeContent } from "./types";
import { ContentMode, ContentTypeId, contentType } from "./registry";

/** Builds the minimal typed content for a type — the discriminator plus its required fields blank. Used to seed a fresh content type. */
export function emptyContent(id: ContentTypeId): CodeContent {
  switch (id) {
    case ContentTypeId.Url:
      return { type: "url", url: "" };
    case ContentTypeId.MobileApp:
      return { type: "mobileApp" };
    case ContentTypeId.Text:
      return { type: "text", text: "" };
    case ContentTypeId.Email:
      return { type: "email", to: "" };
    case ContentTypeId.Sms:
      return { type: "sms", phone: "" };
    case ContentTypeId.Phone:
      return { type: "phone", phone: "" };
    case ContentTypeId.Geo:
      return { type: "geo", latitude: "", longitude: "" };
    case ContentTypeId.Wifi:
      return { type: "wifi", ssid: "", hidden: false };
    case ContentTypeId.VCard:
      return { type: "vcard", firstName: "" };
    case ContentTypeId.Calendar:
      return { type: "calendar", title: "", start: "" };
  }
}

/** A code resolves through its redirect short link (dynamic) rather than a baked payload — true for url / mobileApp / legacy-null content. */
export function isDynamicContent(content: CodeContent | null): boolean {
  return content == null || contentType(content.type).mode === ContentMode.Dynamic;
}

/** Whether a content type resolves through the forwarder short link (dynamic) rather than baking its payload (static). */
export function isDynamicType(id: ContentTypeId): boolean {
  return contentType(id).mode === ContentMode.Dynamic;
}
