-- ============================================================
-- 006-content-model — retire the opaque content descriptor for typed polymorphic content, then make content mandatory.
-- content_json changes SHAPE (no DDL — jsonb is schemaless): the old { type, fields, payload? } becomes a
-- discriminated CodeContent — { "type": "wifi", "ssid": … } — where the backend owns encoding (payload is
-- derived via CodeContent.Encode(), never stored) and the "type" discriminator is the camelCase content id.
-- The old shape can't deserialize into the new hierarchy (enum-name vs camelCase discriminator), so it is replaced.
-- Every code must now carry a typed content: backfill any null OR old-shape row to a url content built from the
-- code's fallback (a plain URL code over its redirect), then enforce NOT NULL.
-- The retired shape is detected by its `fields` key; the new shape has no `fields`. fallback_url is the column.
-- ============================================================

UPDATE codes
SET content_json = jsonb_build_object('type', 'url', 'url', fallback_url)
WHERE content_json IS NULL OR content_json ? 'fields';

ALTER TABLE codes ALTER COLUMN content_json SET NOT NULL;
