-- ============================================================
-- 006-content-model — Rollback.
-- Relax the mandatory content constraint back to nullable. The jsonb SHAPE change has no DDL to reverse and the
-- backfilled url contents are left in place (a null content_json would resolve as a dynamic short link anyway).
-- ============================================================

ALTER TABLE codes ALTER COLUMN content_json DROP NOT NULL;
