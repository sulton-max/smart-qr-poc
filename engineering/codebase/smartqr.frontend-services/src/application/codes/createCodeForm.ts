// The code builder's form module — the whole-form `CreateCodeSchema`, the blank/prefill request factories,
// and the submit normalizer. The form binds `CodeCreateUpdateApiRequest` directly (no separate `*Values` type).

import { z } from "zod";
import { Temporal } from "temporal-polyfill";
import { defaultMapFieldPath } from "@wow-two-beta/ui/forms-engine";

import {
  BarcodeFormat,
  CodeRuleType,
  ContentMode,
  ContentType,
  EccLevel,
  FinderShape,
  MobileAppStoreType,
  ModuleShape,
  RuleConditionType,
  WifiEncryption,
  defaultCodeStyle,
  emptyContent,
  type CodeDto,
  type CodeEmojiDto,
  type CodeLogoDto,
  type CodeRuleDto,
  type ConditionalRuleDto,
  type DefaultRuleDto,
  type Gradient,
} from "@/domain/codes";
import type { CodeCreateUpdateApiRequest } from "@/integration/codes";

/** A zod schema over a const-object enum's values, typed as the exact string-literal union it produces. */
function enumOf<T extends Record<string, string>>(source: T) {
  const values = Object.values(source);
  return z.custom<T[keyof T]>((value) => typeof value === "string" && values.includes(value));
}

// ── Schema ──────────────────────────────────────────────────────────────────────
// `content` is a discriminated union over `type`, each member mirroring its domain interface — payload
// validation is the backend's; the form only shapes what the controls bind to.

const contentSchema = z.discriminatedUnion("type", [
  z.object({ type: z.literal(ContentType.Url), url: z.string() }),
  z.object({
    type: z.literal(ContentType.MobileApp),
    store: enumOf(MobileAppStoreType),
    url: z.string(),
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
  z.object({ type: z.literal(ContentType.Geo), latitude: z.number(), longitude: z.number() }),
  z.object({
    type: z.literal(ContentType.Wifi),
    ssid: z.string(),
    password: z.string().optional(),
    encryption: enumOf(WifiEncryption),
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
    start: z.custom<Temporal.PlainDateTime>((value) => value instanceof Temporal.PlainDateTime),
    end: z.custom<Temporal.PlainDateTime>((value) => value instanceof Temporal.PlainDateTime).optional(),
    location: z.string().optional(),
    description: z.string().optional(),
  }),
]);

// A rule is its role plus the content it serves — the catch-all is a role, never a condition.
const codeRuleSchema = z.discriminatedUnion("type", [
  z.object({
    type: z.literal(CodeRuleType.Conditional),
    order: z.number(),
    condition: enumOf(RuleConditionType),
    conditionValue: z.string(),
    content: contentSchema,
  }),
  z.object({ type: z.literal(CodeRuleType.Default), content: contentSchema }),
  z.object({ type: z.literal(CodeRuleType.DefaultPointer), targetOrder: z.number() }),
]);

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

/** Whole-form validator — the code's identity plus the rules carrying its content. */
export const CreateCodeSchema = z.object({
  name: z.string(),
  barcodeFormat: enumOf(BarcodeFormat),
  mode: enumOf(ContentMode),
  contentType: enumOf(ContentType),
  style: codeStyleSchema,
  rules: z.array(codeRuleSchema).min(1),
});

// ── Factories + mappers ─────────────────────────────────────────────────────────

/** The blank builder request for create mode — a static `url` code carrying one default rule. */
export function emptyCodeCreateUpdateApiRequest(): CodeCreateUpdateApiRequest {
  return {
    name: "",
    barcodeFormat: BarcodeFormat.QrCode,
    mode: ContentMode.Static,
    contentType: ContentType.Url,
    style: { ...defaultCodeStyle },
    rules: [{ type: CodeRuleType.Default, content: { type: ContentType.Url, url: "https://example.com" } }],
  };
}

/** A fresh conditional rule for `useFieldArray('rules').push` — `order` is reassigned by row index at submit. */
export function emptyConditionalRule(contentType: ContentType): ConditionalRuleDto {
  return {
    type: CodeRuleType.Conditional,
    order: 0,
    condition: RuleConditionType.Device,
    conditionValue: "",
    content: emptyContent(contentType),
  };
}

/** A fresh catch-all rule — serves whatever the conditional rules did not match. At most one per code. */
export function emptyDefaultRule(contentType: ContentType): DefaultRuleDto {
  return { type: CodeRuleType.Default, content: emptyContent(contentType) };
}

/** The other side of the mode axis — what an opposite-mode copy is created as (CM5). */
export function oppositeMode(mode: ContentMode): ContentMode {
  return mode === ContentMode.Static ? ContentMode.Dynamic : ContentMode.Static;
}

/**
 * Maps a loaded code to the builder request for edit-mode prefill — feed to `form.reset(...)`.
 * Carries the persisted `mode` even though the update body drops it: the builder needs it to know what the
 * symbol bakes, and mode is locked after create (CM3), so it is the code's mode or nothing.
 */
export function toCodeCreateUpdateApiRequest(code: CodeDto): CodeCreateUpdateApiRequest {
  return {
    name: code.name,
    barcodeFormat: code.barcodeFormat,
    mode: code.mode,
    contentType: code.contentType,
    style: { ...code.style },
    rules: code.rules.map((rule) => ({ ...rule })),
  };
}

/**
 * Maps a loaded code to the builder request for an opposite-mode copy (CM5) — same content and style, a new
 * symbol. The two modes bake different bytes, so a copy is the only way across the axis; the original is untouched.
 */
export function toCopyCodeCreateUpdateApiRequest(code: CodeDto, mode: ContentMode): CodeCreateUpdateApiRequest {
  return {
    name: `${code.name} copy`,
    barcodeFormat: code.barcodeFormat,
    mode,
    contentType: code.contentType,
    style: { ...code.style },
    rules: code.rules.map((rule) => ({ ...rule })),
  };
}

// A rule's content binds as one object (`rules[0].content`) because the per-type `*Controls` are dumb
// value/onChange groups, so no form field exists at `rules[0].content.url`. The server validates the leaf
// and reports the leaf path, which would otherwise be filed at a path nothing subscribes to and never render.
const ContentLeafPath = /^(rules\[\d+]\.content)\..+$/;

/**
 * Rewrites a server error path onto a bound form path — camelCase per segment, then collapses a content
 * leaf onto the object the controls actually bind. The message lands on the right rule's content group.
 */
export function mapCodeFieldPath(serverPath: string): string {
  return defaultMapFieldPath(serverPath).replace(ContentLeafPath, "$1");
}

/** Normalizes the builder request for a create submit — trims the name, renumbers the conditional rules, applies CM2. */
export function toCreateCodeRequest(values: CodeCreateUpdateApiRequest): CodeCreateUpdateApiRequest {
  let order = 0;
  const rules: CodeRuleDto[] = values.rules.map((rule) =>
    rule.type === CodeRuleType.Conditional ? { ...rule, order: ++order } : { ...rule },
  );

  return {
    ...values,
    name: values.name.trim() || "Untitled code",
    // CM2 — two destinations can't be baked into one symbol, so 2+ rules force dynamic. Mirrors the server's check.
    mode: rules.length > 1 ? ContentMode.Dynamic : values.mode,
    rules,
  };
}

/**
 * Normalizes the builder request for an update submit — the create body minus `mode`, which the update contract
 * does not carry (CM3). The form holds the persisted mode so the builder can reason about it; the wire never sees it.
 */
export function toUpdateCodeRequest(values: CodeCreateUpdateApiRequest): Omit<CodeCreateUpdateApiRequest, "mode"> {
  const { mode: _mode, ...request } = toCreateCodeRequest(values);
  return request;
}
