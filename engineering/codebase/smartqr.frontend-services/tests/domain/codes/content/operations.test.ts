import { describe, expect, it } from "vitest";

import { ContentType, contentTypeCatalog, emptyContent, isDynamicContent, isDynamicType } from "@/domain/codes/content";

// Payload ENCODING lives on the backend (SmartQr.Tests.Unit/CodeContentEncodeTests); each type's fields are
// edited via its typed control, so the builder's `CodeContent` is already the wire shape (no flat-values mapping).

describe("emptyContent", () => {
  it("creates a minimal typed content — discriminator set, required fields blank", () => {
    expect(emptyContent(ContentType.Url)).toEqual({ type: "url", url: "" });
    expect(emptyContent(ContentType.Wifi)).toEqual({ type: "wifi", ssid: "", hidden: false });
    expect(emptyContent(ContentType.MobileApp)).toEqual({ type: "mobileApp" });
    expect(emptyContent(ContentType.Geo)).toEqual({ type: "geo", latitude: "", longitude: "" });
  });

  it("covers every registered content type (id ≡ discriminator)", () => {
    for (const c of contentTypeCatalog) {
      expect(emptyContent(c.id).type).toBe(c.id);
    }
  });
});

describe("isDynamicType / isDynamicContent", () => {
  it("url / mobileApp are dynamic; static types + null are classified correctly", () => {
    expect(isDynamicType(ContentType.Url)).toBe(true);
    expect(isDynamicType(ContentType.MobileApp)).toBe(true);
    expect(isDynamicType(ContentType.Wifi)).toBe(false);

    expect(isDynamicContent(null)).toBe(true);
    expect(isDynamicContent({ type: "url", url: "https://x" })).toBe(true);
    expect(isDynamicContent({ type: "vCard", firstName: "Ada" })).toBe(false);
  });
});

describe("contentTypeCatalog registry", () => {
  it("has unique ids", () => {
    const ids = contentTypeCatalog.map((c) => c.id);
    expect(new Set(ids).size).toBe(ids.length);
  });
});
