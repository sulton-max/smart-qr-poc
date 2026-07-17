-- ============================================================
-- 009-drop-code-type — Rollback: restore the code_type column.
-- Re-added with DEFAULT 'qr' so existing rows satisfy NOT NULL; 'qr' is the snake-case form the 004
-- re-encode left behind. The value stays cosmetic until the pre-009 CodeType model is also restored.
-- ============================================================

ALTER TABLE codes ADD COLUMN code_type text NOT NULL DEFAULT 'qr';
