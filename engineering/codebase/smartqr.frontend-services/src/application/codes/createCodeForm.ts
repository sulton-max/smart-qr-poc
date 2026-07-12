// The code builder's form module — the whole-form `CreateCodeSchema` (a zod discriminated content union + a
// routing-rules array), blank/prefill factories, and the submit mapper to the wire request.

import { z } from "zod";

import {
  BarcodeFormat,
  CodeType,
  EccLevel,
  FinderShape,
  ModuleShape,
  defaultCodeStyle,
  type CodeDto,
  type CodeEmojiDto,
  type CodeLogoDto,
  type Gradient,
} from "@/domain/codes";
import { ContentType, isDynamicType } from "@/domain/codes/content";
import { RuleConditionType } from "@/domain/codes/rules";
import type { CodeCreateUpdateApiRequest } from "@/integration/codes";

import type { CodeCreateUpdateFormValues, CodeRuleFormValue } from "./models";

/** A zod schema over a const-object enum's values, typed as the exact string-literal union it produces. */
function enumOf<T extends Record<string, string>>(source: T) {
  const values = Object.values(source);
  return z.custom<T[keyof T]>((value) => typeof value === "string" && values.includes(value));
}

// ── Schema ──────────────────────────────────────────────────────────────────────
// `content` is a discriminated union over `type`; each member mirrors its domain interface. Per-content-field
// messages are omitted — the content sub-controls bind the whole `content` object, so there's no field subscriber
// to render them; content-payload validation is left to the backend. Rule-row destinations DO render (own paths).

const contentSchema = z.discriminatedUnion("type", [
  z.object({ type: z.literal(ContentType.Url), url: z.string() }),
  z.object({
    type: z.literal(ContentType.MobileApp),
    appStore: z.string().optional(),
    playStore: z.string().optional(),
    other: z.string().optional(),
    fallback: z.string().optional(),
  }),
  z.object({ type: z.literal(ContentType.Text), text: z.string() }),
  z.object({
    type: z.literal(ContentType.Email),
    to: z.string(),
    subject: z.string().optional(),
    body: z.string().optional(),
  }),
  z.object({ type: z.literal(ContentType.Sms), phone: z.string(), message: z.string().optional() }),
  z.object({ type: z.literal(ContentType.Phone), phone: z.string() }),
  z.object({ type: z.literal(ContentType.Geo), latitude: z.string(), longitude: z.string() }),
  z.object({
    type: z.literal(ContentType.Wifi),
    ssid: z.string(),
    password: z.string().optional(),
    encryption: z.string().optional(),
    hidden: z.boolean(),
  }),
  z.object({
    type: z.literal(ContentType.VCard),
    firstName: z.string(),
    lastName: z.string().optional(),
    org: z.string().optional(),
    title: z.string().optional(),
    phone: z.string().optional(),
    email: z.string().optional(),
    url: z.string().optional(),
    address: z.string().optional(),
    note: z.string().optional(),
  }),
  z.object({
    type: z.literal(ContentType.Calendar),
    title: z.string(),
    start: z.string(),
    end: z.string().optional(),
    location: z.string().optional(),
    description: z.string().optional(),
  }),
]);

const ruleFormValueSchema = z.object({
  id: z.string(),
  order: z.number(),
  conditionType: enumOf(RuleConditionType),
  conditionValue: z.string(),
  destination: z.string(),
});

const codeStyleSchema = z.object({
  foregroundColor: z.string(),
  backgroundColor: z.string(),
  transparentBackground: z.boolean(),
  eccLevel: enumOf(EccLevel),
  quietZoneModules: z.number(),
  // Complex external color/emoji/logo models — carried by type (validated by their own controls), not re-parsed.
  logo: z.custom<CodeLogoDto>().optional(),
  moduleShape: enumOf(ModuleShape),
  finderShape: enumOf(FinderShape),
  finderDotShape: enumOf(FinderShape),
  gradient: z.custom<Gradient>().optional(),
  emoji: z.custom<CodeEmojiDto>().optional(),
});

/** Whole-form validator — discriminated content union + rules array, with content-gated row validation. */
export const CreateCodeSchema = z
  .object({
    name: z.string(),
    codeType: enumOf(CodeType),
    barcodeFormat: enumOf(BarcodeFormat),
    fallbackUrl: z.string(),
    content: contentSchema,
    style: codeStyleSchema,
    rules: z.array(ruleFormValueSchema),
  })
  .superRefine((values, ctx) => {
    // Rules only route a plain URL forwarder — static + mobileApp derive routing server-side and the submit drops
    // the array. Validate row destinations only when they'll actually be sent (cross-section refine).
    if (values.content.type !== ContentType.Url) return;
    values.rules.forEach((rule, index) => {
      if (!rule.destination.trim()) {
        ctx.addIssue({
          code: "custom",
          path: ["rules", index, "destination"],
          message: "Add a destination URL for this rule.",
        });
      }
    });
  });

// ── Factories + mappers ─────────────────────────────────────────────────────────

/** The blank builder values for create mode — a `url` forwarder seeded with a sample so the preview renders. */
export function emptyCreateCodeFormValues(): CodeCreateUpdateFormValues {
  return {
    name: "",
    codeType: CodeType.Qr,
    barcodeFormat: BarcodeFormat.QrCode,
    fallbackUrl: "",
    content: { type: ContentType.Url, url: "https://example.com" },
    // Seed the house look (rounded + black→violet radial) so a new code is polished on first render.
    style: { ...defaultCodeStyle },
    rules: [],
  };
}

/** A fresh empty routing rule for `form.array('rules').push` — `order` is assigned by row index at submit. */
export function emptyRuleFormValue(): CodeRuleFormValue {
  return {
    id: crypto.randomUUID(),
    order: 0,
    conditionType: RuleConditionType.Device,
    conditionValue: "",
    destination: "",
  };
}

/** Maps a loaded code to builder values for edit-mode prefill — feed to `form.reset(...)` (no per-field setters). */
export function toCreateCodeFormValues(code: CodeDto): CodeCreateUpdateFormValues {
  return {
    name: code.name,
    codeType: CodeType.Qr,
    barcodeFormat: code.barcodeFormat,
    fallbackUrl: code.fallbackUrl,
    // A legacy code (no typed content) opens as an editable `url` forwarder seeded from its stored fallback.
    content: code.content ?? { type: ContentType.Url, url: code.fallbackUrl },
    style: { ...code.style },
    rules: code.rules.map((rule) => ({
      id: crypto.randomUUID(),
      order: rule.order,
      conditionType: rule.conditionType,
      conditionValue: rule.conditionValue ?? "",
      destination: rule.destination,
    })),
  };
}

/** Resolves builder values to the wire request — re-trims (the schema validates only) + gates the content routing rules, dropping the row keys. */
export function toCreateCodeRequest(values: CodeCreateUpdateFormValues): CodeCreateUpdateApiRequest {
  const { name, barcodeFormat, content, style, rules } = values;
  // Content shapes the request: static bakes its payload server-side (no redirect, no rules); a self-routed type
  // (mobileApp) derives its fallback + device rules server-side; a plain URL keeps its own routing rules.
  const isStatic = !isDynamicType(content.type);
  const selfRouted = content.type === ContentType.MobileApp;
  return {
    name: name.trim() || "Untitled code",
    codeType: CodeType.Qr,
    barcodeFormat,
    fallbackUrl:
      isStatic || selfRouted ? "" : content.type === ContentType.Url ? content.url.trim() : "",
    rules:
      isStatic || selfRouted
        ? []
        : rules.map((rule, index) => ({
            order: index + 1,
            conditionType: rule.conditionType,
            conditionValue: rule.conditionValue.trim(),
            destination: rule.destination.trim(),
          })),
    style,
    content,
  };
}
