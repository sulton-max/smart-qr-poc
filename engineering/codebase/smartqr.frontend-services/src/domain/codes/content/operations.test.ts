import { describe, expect, it } from "vitest";
import { ContentTypeId, ContentTypes } from "./registry";
import { emptyContent, isDynamicContent, isDynamicType } from "./operations";

// Payload ENCODING lives on the backend (SmartQr.Tests.Unit/CodeContentEncodeTests); each type's fields are
// edited via its typed control, so the builder's `CodeContent` is already the wire shape (no flat-values mapping).

describe("emptyContent", () => {
  it("creates a minimal typed content — discriminator set, required fields blank", () => {
    expect(emptyContent(ContentTypeId.Url)).toEqual({ type: "url", url: "" });
    expect(emptyContent(ContentTypeId.Wifi)).toEqual({ type: "wifi", ssid: "", hidden: false });
    expect(emptyContent(ContentTypeId.MobileApp)).toEqual({ type: "mobileApp" });
    expect(emptyContent(ContentTypeId.Geo)).toEqual({ type: "geo", latitude: "", longitude: "" });
  });

  it("covers every registered content type (id ≡ discriminator)", () => {
    for (const c of ContentTypes) {
      expect(emptyContent(c.id).type).toBe(c.id);
    }
  });
});

describe("isDynamicType / isDynamicContent", () => {
  it("url / mobileApp are dynamic; static types + null are classified correctly", () => {
    expect(isDynamicType(ContentTypeId.Url)).toBe(true);
    expect(isDynamicType(ContentTypeId.MobileApp)).toBe(true);
    expect(isDynamicType(ContentTypeId.Wifi)).toBe(false);

    expect(isDynamicContent(null)).toBe(true);
    expect(isDynamicContent({ type: "url", url: "https://x" })).toBe(true);
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
