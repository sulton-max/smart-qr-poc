// The code builder's form model — the editable `CreateCodeValues` shape, its whole-form `CreateCodeSchema`
// (a zod discriminated content union + a routing-rules array), and the value ↔ wire mappers. Bound to the
// builder via `useAppForm` (see wow-two-ws conventions/development/frontend/presentation/forms.md).
//
// Shape choice: design settings are grouped under one `style` sub-object (mirrors the wire `PreviewStyle`
// aggregate + the content controls' whole-object edit idiom) so the design panel binds through a single
// `form.Field name="style"` instead of eight sibling fields. `content` stays its own field — a discriminated
// union edited as one typed object. `rules` is a row array driven by `form.array`.

import { z } from "zod";

import {
  BarcodeFormat,
  CodeType,
  EccLevel,
  FinderShape,
  GradientType,
  ModuleShape,
  defaultCodeStyle,
  type CodeDto,
  type CreateCodeRequest,
  type DesignEmojiOverlay,
  type Gradient,
  type PreviewStyle,
  type RuleDraft,
} from "@/domain/codes";
import { ContentTypeId, isDynamicType, type CodeContent } from "@/domain/codes/content";
import { RuleConditionType } from "@/domain/codes/rules";

/** A zod schema over a const-object enum's values, typed as the exact string-literal union it produces. */
function enumOf<T extends Record<string, string>>(source: T) {
  const values = Object.values(source);
  return z.custom<T[keyof T]>((value) => typeof value === "string" && values.includes(value));
}

/** The design settings the builder edits — the granular inputs behind the wire `PreviewStyle`. */
export interface BuilderStyle {
  /** The solid foreground color (#RRGGBB). */
  foreground: string;
  /** The background color (#RRGGBB). */
  background: string;
  /** Whether the background is transparent (overrides `background`). */
  transparentBackground: boolean;
  /** The data-module shape. */
  moduleShape: ModuleShape;
  /** The finder (eye) frame shape. */
  finderShape: FinderShape;
  /** The finder (eye) pupil shape. */
  finderDotShape: FinderShape;
  /** The foreground gradient, or null for a solid fill. */
  gradient: Gradient | null;
  /** The center emoji overlay, or null for none. */
  emoji: DesignEmojiOverlay | null;
}

/** The editable code-builder form values — bound to `useAppForm`, resolved to a `CreateCodeRequest` in `onSubmit`. */
export interface CreateCodeValues {
  /** The code's display name (blank → "Untitled code" on submit). */
  name: string;
  /** The barcode symbology (QR / 1D / 2D). */
  symbology: BarcodeFormat;
  /** The typed content the code carries (discriminated on `type`). */
  content: CodeContent;
  /** The visual design settings. */
  style: BuilderStyle;
  /** The ordered routing rules (first match wins). */
  rules: RuleDraft[];
}

// ── Schema ──────────────────────────────────────────────────────────────────────
// One whole-form schema. `content` is a discriminated union over `type`; each member mirrors its domain
// interface (required fields as `z.string()`, optional as `.optional()`). Per-content-field messages are
// intentionally omitted — the content sub-controls bind to the whole `content` object (one `form.Field`),
// so a `content.url` issue has no field subscriber to render it; content-payload validation is left to the
// backend, surfaced via `submitError`. Rule-row destinations ARE validated (and DO render) because each
// row cell is its own `form.Field` deep path.

const contentSchema = z.discriminatedUnion("type", [
  z.object({ type: z.literal(ContentTypeId.Url), url: z.string() }),
  z.object({
    type: z.literal(ContentTypeId.MobileApp),
    appStore: z.string().optional(),
    playStore: z.string().optional(),
    other: z.string().optional(),
    fallback: z.string().optional(),
  }),
  z.object({ type: z.literal(ContentTypeId.Text), text: z.string() }),
  z.object({
    type: z.literal(ContentTypeId.Email),
    to: z.string(),
    subject: z.string().optional(),
    body: z.string().optional(),
  }),
  z.object({ type: z.literal(ContentTypeId.Sms), phone: z.string(), message: z.string().optional() }),
  z.object({ type: z.literal(ContentTypeId.Phone), phone: z.string() }),
  z.object({ type: z.literal(ContentTypeId.Geo), latitude: z.string(), longitude: z.string() }),
  z.object({
    type: z.literal(ContentTypeId.Wifi),
    ssid: z.string(),
    password: z.string().optional(),
    encryption: z.string().optional(),
    hidden: z.boolean(),
  }),
  z.object({
    type: z.literal(ContentTypeId.VCard),
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
    type: z.literal(ContentTypeId.Calendar),
    title: z.string(),
    start: z.string(),
    end: z.string().optional(),
    location: z.string().optional(),
    description: z.string().optional(),
  }),
]);

const ruleDraftSchema = z.object({
  id: z.string(),
  order: z.number(),
  conditionType: enumOf(RuleConditionType),
  conditionValue: z.string(),
  destination: z.string(),
});

const builderStyleSchema = z.object({
  foreground: z.string(),
  background: z.string(),
  transparentBackground: z.boolean(),
  moduleShape: enumOf(ModuleShape),
  finderShape: enumOf(FinderShape),
  finderDotShape: enumOf(FinderShape),
  // Complex external color/emoji models — carried by type (validated by their own controls), not re-parsed here.
  gradient: z.custom<Gradient | null>(),
  emoji: z.custom<DesignEmojiOverlay | null>(),
});

/** Whole-form validator — discriminated content union + rules array, with content-gated row validation. */
export const CreateCodeSchema = z
  .object({
    name: z.string(),
    symbology: enumOf(BarcodeFormat),
    content: contentSchema,
    style: builderStyleSchema,
    rules: z.array(ruleDraftSchema),
  })
  .superRefine((values, ctx) => {
    // Rules only route a plain URL forwarder — static + mobileApp derive routing server-side and the submit
    // drops the array. Validate row destinations only when they'll actually be sent (cross-section refine).
    if (values.content.type !== ContentTypeId.Url) return;
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
export function emptyCreateCodeValues(): CreateCodeValues {
  return {
    name: "",
    symbology: BarcodeFormat.QrCode,
    content: { type: ContentTypeId.Url, url: "https://example.com" },
    // Seed the house look (rounded + black→violet radial) so a new code is polished on first render.
    style: {
      foreground: defaultCodeStyle.foregroundColor,
      background: defaultCodeStyle.backgroundColor,
      transparentBackground: defaultCodeStyle.transparentBackground,
      moduleShape: defaultCodeStyle.moduleShape,
      finderShape: defaultCodeStyle.finderShape,
      finderDotShape: defaultCodeStyle.finderDotShape,
      gradient: defaultCodeStyle.gradient,
      emoji: defaultCodeStyle.emoji,
    },
    rules: [],
  };
}

/** A fresh empty routing rule for `form.array('rules').push` — `order` is assigned by row index at submit. */
export function emptyRuleDraft(): RuleDraft {
  return {
    id: crypto.randomUUID(),
    order: 0,
    conditionType: RuleConditionType.Device,
    conditionValue: "",
    destination: "",
  };
}

/** Maps a persisted gradient (wire shape) back to the builder's gradient, or null for a solid foreground. */
function gradientFromDto(dto: CodeDto["style"]["gradient"]): Gradient | null {
  if (!dto) return null;
  const stops = dto.stops.map((stop) => ({ color: stop.color, offset: stop.offset }));
  return dto.type === GradientType.Linear
    ? { type: GradientType.Linear, angle: dto.angle, stops }
    : { type: GradientType.Radial, radius: dto.radius, stops };
}

/** Maps a code's persisted rules to the builder's draft rows (adds client-side keys). */
function toRuleDrafts(code: CodeDto): RuleDraft[] {
  return code.rules.map((rule) => ({
    id: crypto.randomUUID(),
    order: rule.order,
    conditionType: rule.conditionType,
    conditionValue: rule.conditionValue ?? "",
    destination: rule.destination,
  }));
}

/** Maps a loaded code to builder values for edit-mode prefill — feed to `form.reset(...)` (no per-field setters). */
export function toCreateCodeValues(code: CodeDto): CreateCodeValues {
  return {
    name: code.name,
    symbology: code.barcodeFormat,
    // A legacy code (no typed content) opens as an editable `url` forwarder seeded from its stored fallback.
    content: code.content ?? { type: ContentTypeId.Url, url: code.fallbackUrl },
    style: {
      foreground: code.style.foregroundColor,
      background: code.style.backgroundColor,
      transparentBackground: code.style.transparentBackground,
      moduleShape: code.style.moduleShape,
      finderShape: code.style.finderShape,
      finderDotShape: code.style.finderDotShape,
      gradient: gradientFromDto(code.style.gradient),
      emoji: code.style.emoji ?? null,
    },
    rules: toRuleDrafts(code),
  };
}

/** Assembles the full render `PreviewStyle` from the builder's design settings (adds the unsurfaced defaults). */
export function toPreviewStyle(style: BuilderStyle): PreviewStyle {
  return {
    foregroundColor: style.foreground,
    backgroundColor: style.background,
    transparentBackground: style.transparentBackground,
    // Defaults (ECC / quiet-zone / logo) aren't surfaced in the builder yet — send the renderer's standard values.
    eccLevel: EccLevel.Q,
    quietZoneModules: 2,
    logo: null,
    moduleShape: style.moduleShape,
    finderShape: style.finderShape,
    finderDotShape: style.finderDotShape,
    gradient: style.gradient,
    emoji: style.emoji,
  };
}

/** Resolves builder values to the wire request — re-trims (the schema validates only) + applies the content routing rules. */
export function toCreateCodeRequest(values: CreateCodeValues): CreateCodeRequest {
  const { name, symbology, content, style, rules } = values;
  // Content shapes the request: static bakes its payload server-side (no redirect, no rules); a self-routed
  // type (mobileApp) derives its fallback + device rules server-side; a plain URL keeps its own routing rules.
  const isStatic = !isDynamicType(content.type);
  const selfRouted = content.type === ContentTypeId.MobileApp;
  return {
    name: name.trim() || "Untitled code",
    codeType: CodeType.Qr,
    barcodeFormat: symbology,
    fallbackUrl:
      isStatic || selfRouted ? "" : content.type === ContentTypeId.Url ? content.url.trim() : "",
    rules:
      isStatic || selfRouted
        ? []
        : rules.map((rule, index) => ({
            order: index + 1,
            conditionType: rule.conditionType,
            conditionValue: rule.conditionValue.trim(),
            destination: rule.destination.trim(),
          })),
    style: toPreviewStyle(style),
    content,
  };
}
