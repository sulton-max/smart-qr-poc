-- ============================================================
-- 008-drop-expiry — Rollback: restore the never_expires + expires_at columns.
-- never_expires comes back with DEFAULT true so existing rows satisfy NOT NULL and keep the
-- never-expire promise; expires_at is nullable and stays empty. Both are cosmetic until the
-- pre-008 expiry gate in RoutingService is also restored.
-- ============================================================

ALTER TABLE codes ADD COLUMN never_expires boolean NOT NULL DEFAULT true;
ALTER TABLE codes ADD COLUMN expires_at timestamptz NULL;
