import { describe, expect, it } from "vitest";
import { CONTENT_TYPES, encodeContent, toICalDate } from "./contentTypes";

describe("encodeContent", () => {
  it("url / text are passed through (trimmed for url)", () => {
    expect(encodeContent("url", { url: "  https://x.io  " })).toBe("https://x.io");
    expect(encodeContent("text", { text: "hi there" })).toBe("hi there");
  });

  it("phone → tel:, sms → SMSTO: (message optional)", () => {
    expect(encodeContent("phone", { phone: "+15550100" })).toBe("tel:+15550100");
    expect(encodeContent("sms", { phone: "+15550100" })).toBe("SMSTO:+15550100");
    expect(encodeContent("sms", { phone: "+15550100", message: "hey" })).toBe("SMSTO:+15550100:hey");
  });

  it("email → mailto: with encoded subject/body params", () => {
    expect(encodeContent("email", { to: "a@b.com" })).toBe("mailto:a@b.com");
    expect(encodeContent("email", { to: "a@b.com", subject: "Hi & bye", body: "line one" })).toBe(
      "mailto:a@b.com?subject=Hi+%26+bye&body=line+one",
    );
  });

  it("geo → geo:lat,lng", () => {
    expect(encodeContent("geo", { lat: "41.31", lng: "69.24" })).toBe("geo:41.31,69.24");
  });

  it("wifi → WIFI: with escaping; nopass drops the password; hidden flag", () => {
    expect(encodeContent("wifi", { ssid: "Cafe", password: "p@ss", encryption: "WPA" })).toBe(
      "WIFI:T:WPA;S:Cafe;P:p@ss;;",
    );
    // reserved chars in SSID/password are backslash-escaped
    expect(encodeContent("wifi", { ssid: "My;Net", password: 'a"b,c', encryption: "WPA" })).toBe(
      'WIFI:T:WPA;S:My\\;Net;P:a\\"b\\,c;;',
    );
    expect(encodeContent("wifi", { ssid: "Open", encryption: "nopass" })).toBe("WIFI:T:nopass;S:Open;;");
    expect(encodeContent("wifi", { ssid: "Hid", password: "x", encryption: "WPA", hidden: "true" })).toBe(
      "WIFI:T:WPA;S:Hid;P:x;H:true;;",
    );
  });

  it("vcard → vCard 3.0 with only filled fields, ical-escaped", () => {
    const out = encodeContent("vcard", { firstName: "Ada", lastName: "Lovelace", org: "Babbage, Inc", phone: "+15550100" });
    expect(out).toContain("BEGIN:VCARD");
    expect(out).toContain("VERSION:3.0");
    expect(out).toContain("N:Lovelace;Ada;;;");
    expect(out).toContain("FN:Ada Lovelace");
    expect(out).toContain("ORG:Babbage\\, Inc");
    expect(out).toContain("TEL;TYPE=CELL:+15550100");
    expect(out).not.toContain("EMAIL:");
    expect(out.endsWith("END:VCARD")).toBe(true);
  });

  it("calendar → VEVENT with basic-format dates", () => {
    const out = encodeContent("calendar", { title: "Launch", start: "2026-07-01T18:30", end: "2026-07-01T19:00", location: "HQ" });
    expect(out).toContain("BEGIN:VEVENT");
    expect(out).toContain("SUMMARY:Launch");
    expect(out).toContain("DTSTART:20260701T183000");
    expect(out).toContain("DTEND:20260701T190000");
    expect(out).toContain("LOCATION:HQ");
    expect(out.endsWith("END:VEVENT")).toBe(true);
  });
});

describe("toICalDate", () => {
  it("formats datetime-local, date-only, and passes through unknown", () => {
    expect(toICalDate("2026-07-01T18:30")).toBe("20260701T183000");
    expect(toICalDate("2026-07-01T18:30:45")).toBe("20260701T183045");
    expect(toICalDate("2026-07-01")).toBe("20260701");
    expect(toICalDate("")).toBe("");
  });
});

describe("CONTENT_TYPES registry", () => {
  it("every type has a non-empty encode + at least one required field, and ids are unique", () => {
    const ids = CONTENT_TYPES.map((c) => c.id);
    expect(new Set(ids).size).toBe(ids.length);
    for (const c of CONTENT_TYPES) {
      expect(typeof c.encode).toBe("function");
      expect(c.fields.length).toBeGreaterThan(0);
    }
  });
});
