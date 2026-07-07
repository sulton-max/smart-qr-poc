import { describe, expect, it } from "vitest";
import { ContentTypeId, ContentTypes } from "./registry";
import { buildContent, contentToValues, isDynamicContent } from "./operations";

// Payload ENCODING now lives on the backend (SmartQr.Tests.Unit/CodeContentEncodeTests); the frontend only maps
// the builder's flat form values to/from the typed `CodeContent` the wire carries.

describe("buildContent", () => {
  it("passes a type's required field through", () => {
    expect(buildContent(ContentTypeId.Url, { url: "https://x.io" })).toEqual({ type: "url", url: "https://x.io" });
    expect(buildContent(ContentTypeId.Text, { text: "hi there" })).toEqual({ type: "text", text: "hi there" });
    expect(buildContent(ContentTypeId.Phone, { phone: "+15550100" })).toEqual({ type: "phone", phone: "+15550100" });
  });

  it("keeps required fields but omits empty optional ones", () => {
    expect(buildContent(ContentTypeId.Email, { to: "a@b.com", subject: "Hi", body: "" })).toEqual({
      type: "email",
      to: "a@b.com",
      subject: "Hi",
    });
    expect(buildContent(ContentTypeId.Sms, { phone: "+15550100" })).toEqual({ type: "sms", phone: "+15550100" });
  });

  it("geo carries latitude/longitude keys", () => {
    expect(buildContent(ContentTypeId.Geo, { latitude: "41.31", longitude: "69.24" })).toEqual({
      type: "geo",
      latitude: "41.31",
      longitude: "69.24",
    });
  });

  it("wifi maps hidden to a bool (defaulting false) and omits an empty password", () => {
    expect(buildContent(ContentTypeId.Wifi, { ssid: "Cafe", password: "pw", hidden: "true" })).toEqual({
      type: "wifi",
      ssid: "Cafe",
      password: "pw",
      hidden: true,
    });
    expect(buildContent(ContentTypeId.Wifi, { ssid: "Open" })).toEqual({ type: "wifi", ssid: "Open", hidden: false });
  });

  it("mobileApp omits empty links and carries the fallback choice", () => {
    expect(buildContent(ContentTypeId.MobileApp, { appStore: "https://a", fallback: "appStore" })).toEqual({
      type: "mobileApp",
      appStore: "https://a",
      fallback: "appStore",
    });
    // No links → an empty object (backend rejects it) — no stray empty-string fields that would defeat the check.
    expect(buildContent(ContentTypeId.MobileApp, {})).toEqual({ type: "mobileApp" });
  });
});

describe("contentToValues", () => {
  it("round-trips buildContent (wifi bool → 'true'/'false' string)", () => {
    const content = buildContent("wifi", { ssid: "Cafe", password: "pw", hidden: "true" });
    expect(contentToValues(content)).toEqual({ ssid: "Cafe", password: "pw", hidden: "true" });
  });

  it("projects geo latitude/longitude back to the form keys", () => {
    expect(contentToValues({ type: "geo", latitude: "41", longitude: "69" })).toEqual({
      latitude: "41",
      longitude: "69",
    });
  });
});

describe("isDynamicContent", () => {
  it("is true for url / mobileApp / null; false for static types", () => {
    expect(isDynamicContent(null)).toBe(true);
    expect(isDynamicContent({ type: "url", url: "https://x" })).toBe(true);
    expect(isDynamicContent({ type: "mobileApp", appStore: "https://a" })).toBe(true);
    expect(isDynamicContent({ type: "wifi", ssid: "N", hidden: false })).toBe(false);
    expect(isDynamicContent({ type: "vcard", firstName: "Ada" })).toBe(false);
  });
});

describe("ContentTypes registry", () => {
  it("has unique ids, each with at least one field", () => {
    const ids = ContentTypes.map((c) => c.id);
    expect(new Set(ids).size).toBe(ids.length);
    for (const c of ContentTypes) {
      expect(c.fields.length).toBeGreaterThan(0);
    }
  });
});
