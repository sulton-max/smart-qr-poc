-- ============================================================
-- 005-static-content — carry a code's structured content descriptor.
-- content_json holds { type, fields, payload? } as jsonb: `type`/`fields` round-trip the
-- builder form on edit; a non-null `payload` marks a STATIC code whose payload is baked
-- into the symbol (WiFi / vCard / geo / text …) instead of the redirect short link.
-- Nullable — legacy rows (created before content types) resolve as dynamic short links.
-- ============================================================

ALTER TABLE codes ADD COLUMN content_json jsonb NULL;
