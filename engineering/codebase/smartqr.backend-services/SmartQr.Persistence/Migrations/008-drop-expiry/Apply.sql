-- ============================================================
-- 008-drop-expiry — retire the never_expires + expires_at columns.
-- Both were vestigial: never_expires was hardcoded true at create and expires_at had no write
-- path, so the 410 Gone gate could never fire. "Codes never expire" is the product promise, not
-- a per-row flag. Intentional expiration is a separate, deferred feature and will model its own
-- columns. No data migration — no row ever carried a real expiry.
-- ============================================================

ALTER TABLE codes DROP COLUMN never_expires;
ALTER TABLE codes DROP COLUMN expires_at;
