-- ============================================================
-- 005-static-content — Rollback: drop the content descriptor column.
-- Reverting loses baked static payloads; static codes fall back to encoding the short link.
-- ============================================================

ALTER TABLE codes DROP COLUMN content_json;
