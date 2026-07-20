// Content operations — factories over the typed `CodeContent` union a rule carries.

import { Temporal } from "temporal-polyfill";

import type { CodeContent } from "./models";
import { ContentType } from "./enums/ContentType";
import { MobileAppStoreType } from "./enums/MobileAppStoreType";
import { WifiEncryption } from "./enums/WifiEncryption";

/** Builds the minimal typed content for a type — the discriminator plus its required fields blank. Used to seed a fresh content type. */
export function emptyContent(id: ContentType): CodeContent {
  switch (id) {
    case ContentType.Url:
      return { type: ContentType.Url, url: "" };
    case ContentType.MobileApp:
      return { type: ContentType.MobileApp, store: MobileAppStoreType.AppStore, url: "" };
    case ContentType.Text:
      return { type: ContentType.Text, text: "" };
    case ContentType.Email:
      return { type: ContentType.Email, to: "" };
    case ContentType.Sms:
      return { type: ContentType.Sms, phone: "" };
    case ContentType.Phone:
      return { type: ContentType.Phone, phone: "" };
    case ContentType.Geo:
      return { type: ContentType.Geo, latitude: 0, longitude: 0 };
    case ContentType.Wifi:
      return { type: ContentType.Wifi, ssid: "", encryption: WifiEncryption.Wpa, hidden: false };
    case ContentType.VCard:
      return { type: ContentType.VCard, firstName: "" };
    case ContentType.Calendar:
      return { type: ContentType.Calendar, title: "", start: Temporal.Now.plainDateTimeISO().round("minute") };
  }
}
