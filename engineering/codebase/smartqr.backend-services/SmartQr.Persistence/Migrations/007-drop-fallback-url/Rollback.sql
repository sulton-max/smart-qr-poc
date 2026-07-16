-- ============================================================
-- 007-drop-fallback-url — Rollback: restore the fallback_url column.
-- Re-added with a temporary empty default so existing rows satisfy NOT NULL; the routing engine
-- no longer reads it, so the value stays cosmetic until the pre-007 resolver is also restored.
-- ============================================================

ALTER TABLE codes ADD COLUMN fallback_url text NOT NULL DEFAULT '';
