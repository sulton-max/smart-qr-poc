-- ============================================================
-- 006-content-model — retire the opaque content descriptor for typed polymorphic content.
-- content_json changes SHAPE (no DDL — jsonb is schemaless): the old { type, fields, payload? } becomes a
-- discriminated CodeContent — { "type": "wifi", "ssid": … } — where the backend owns encoding (payload is
-- derived via CodeContent.Encode(), never stored) and the "type" discriminator is the camelCase content id.
-- The old shape can't deserialize into the new hierarchy (enum-name vs camelCase discriminator), so wipe it.
-- POC: no production data — a null content_json resolves as a dynamic short link, same as a legacy row.
-- ============================================================

UPDATE codes SET content_json = NULL WHERE content_json IS NOT NULL;
