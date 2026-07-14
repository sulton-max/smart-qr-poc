// The code builder's form module — the whole-form `CreateCodeSchema`, the blank/prefill request factories,
// and the submit normalizer. The form binds `CodeCreateUpdateApiRequest` directly (no separate `*Values` type).

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
  type CodeRuleDto,
  type Gradient,
} from "@/domain/codes";
import { ContentType, isDynamicType } from "@/domain/codes/content";
import { RuleConditionType } from "@/domain/codes/rules";
import type { CodeCreateUpdateApiRequest } from "@/integration/codes";

/** A zod schema over a const-object enum's values, typed as the exact string-literal union it produces. */
function enumOf<T extends Record<string, string>>(source: T) {
  const values = Object.values(source);
  return z.custom<T[keyof T]>((value) => typeof value === "string" && values.includes(value));
}

// ── Schema ──────────────────────────────────────────────────────────────────────
// `content` is a discriminated union over `type`, each member mirroring its domain interface — payload
// validation is the backend's; only rule-row destinations validate here (they render their own errors).

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

const codeRuleSchema = z.object({
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
    rules: z.array(codeRuleSchema),
  })
  .superRefine((values, ctx) => {
    // Row destinations are sent only for a plain URL forwarder — validate them only then.
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

/** The blank builder request for create mode — a `url` forwarder seeded with a sample so the preview renders. */
export function emptyCodeCreateUpdateApiRequest(): CodeCreateUpdateApiRequest {
  return {
    name: "",
    codeType: CodeType.Qr,
    barcodeFormat: BarcodeFormat.QrCode,
    fallbackUrl: "",
    content: { type: ContentType.Url, url: "https://example.com" },
    style: { ...defaultCodeStyle },
    rules: [],
  };
}

/** A fresh empty routing rule for `useFieldArray('rules').push` — `order` is reassigned by row index at submit. */
export function emptyCodeRule(): CodeRuleDto {
  return {
    order: 0,
    conditionType: RuleConditionType.Device,
    conditionValue: "",
    destination: "",
  };
}

/** Maps a loaded code to the builder request for edit-mode prefill — feed to `form.reset(...)`. */
export function toCodeCreateUpdateApiRequest(code: CodeDto): CodeCreateUpdateApiRequest {
  return {
    name: code.name,
    codeType: CodeType.Qr,
    barcodeFormat: code.barcodeFormat,
    fallbackUrl: code.fallbackUrl,
    // A legacy code (no typed content) opens as an editable `url` forwarder seeded from its stored fallback.
    content: code.content ?? { type: ContentType.Url, url: code.fallbackUrl },
    style: { ...code.style },
    rules: code.rules.map((rule) => ({
      order: rule.order,
      conditionType: rule.conditionType,
      conditionValue: rule.conditionValue ?? "",
      destination: rule.destination,
    })),
  };
}

/** Normalizes the builder request for submit — trims fields and content-gates the routing rules. */
export function toCreateCodeRequest(values: CodeCreateUpdateApiRequest): CodeCreateUpdateApiRequest {
  const { name, barcodeFormat, content, style, rules } = values;
  // Content shapes the request: a static type bakes its payload server-side (no redirect, no rules); a
  // self-routed type (mobileApp) derives its fallback + device rules server-side; a plain URL keeps its rules.
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
            conditionValue: (rule.conditionValue ?? "").trim(),
            destination: rule.destination.trim(),
          })),
    style,
    content,
  };
}
