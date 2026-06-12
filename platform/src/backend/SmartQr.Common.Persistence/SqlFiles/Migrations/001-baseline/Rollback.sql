-- ============================================================
-- 001-baseline rollback — drops everything Apply.sql created.
-- Reverse dependency order; IF EXISTS + CASCADE so it is safe on partial state.
-- Dev/test only — production rolls forward.
-- ============================================================

DROP TABLE IF EXISTS scan_events CASCADE;
DROP TABLE IF EXISTS routing_rules CASCADE;
DROP TABLE IF EXISTS codes CASCADE;
