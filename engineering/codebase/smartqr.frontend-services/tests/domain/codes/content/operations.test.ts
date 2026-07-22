import { describe, expect, it } from "vitest";
import { Temporal } from "temporal-polyfill";

import { ContentType, contentTypeCatalog, emptyContent } from "@/domain/codes/content";

// Payload ENCODING lives on the backend (SmartQr.Tests.Unit/CodeContentEncodeTests); each type's fields are
// edited via its typed control, so the builder's `CodeContent` is already the wire shape (no flat-values mapping).

describe("emptyContent", () => {
  it("creates a minimal typed content — discriminator set, required fields blank", () => {
    expect(emptyContent(ContentType.Url)).toEqual({ type: "url", url: "" });
    expect(emptyContent(ContentType.Wifi)).toEqual({
      type: "wifi",
      ssid: "",
      encryption: "wpa",
      hidden: false,
    });
    expect(emptyContent(ContentType.MobileApp)).toEqual({ type: "mobileApp", store: "appStore", url: "" });
    expect(emptyContent(ContentType.Geo)).toEqual({ type: "geo", latitude: 0, longitude: 0 });
  });

  it("seeds a calendar event with a whole-minute start", () => {
    const content = emptyContent(ContentType.Calendar);
    expect(content.type).toBe(ContentType.Calendar);
    if (content.type !== ContentType.Calendar) return;
    expect(content.start).toBeInstanceOf(Temporal.PlainDateTime);
    expect(content.start.second).toBe(0);
  });

  it("covers every registered content type (id ≡ discriminator)", () => {
    for (const c of contentTypeCatalog) {
      expect(emptyContent(c.id).type).toBe(c.id);
    }
  });
});

describe("contentTypeCatalog registry", () => {
  it("has unique ids", () => {
    const ids = contentTypeCatalog.map((c) => c.id);
    expect(new Set(ids).size).toBe(ids.length);
  });
});
