-- ============================================================
-- 010-rules-jsonb — a rule carries its content; the code no longer does.
--
-- The content model's v2: content moves out of codes.content_json and into each rule, so a rule is a
-- condition plus the content it serves. Static content is content in a default rule.
--
-- Rules become one jsonb document on the code rather than their own table: they are only ever read with
-- their code, never queried alone, and the three rule roles (conditional / default / default-pointer) would
-- each need their own nullable columns in a relational shape.
--
-- Also lands the mode axis: a code's symbol either bakes its payload (static) or carries the short link
-- (dynamic), fixed at create. Only a dynamic code resolves through the redirect, so only a dynamic code
-- gets a slug — hence slug becomes nullable. Postgres treats NULLs as distinct, so the unique index still
-- admits every static code.
--
-- No data migration: the DB is resettable and no rows are preserved.
-- ============================================================

DROP TABLE routing_rules;

ALTER TABLE codes DROP COLUMN content_json;

ALTER TABLE codes ADD COLUMN rules jsonb NOT NULL DEFAULT '[]'::jsonb;
ALTER TABLE codes ALTER COLUMN rules DROP DEFAULT;

ALTER TABLE codes ADD COLUMN mode text NOT NULL DEFAULT 'static';
ALTER TABLE codes ALTER COLUMN mode DROP DEFAULT;

ALTER TABLE codes ADD COLUMN content_type text NOT NULL DEFAULT 'url';
ALTER TABLE codes ALTER COLUMN content_type DROP DEFAULT;

ALTER TABLE codes ALTER COLUMN slug DROP NOT NULL;

-- The matched rule is identified by its order within the code; rule rows no longer carry ids.
ALTER TABLE scan_events DROP COLUMN matched_rule_id;
ALTER TABLE scan_events ADD COLUMN matched_rule_order integer NULL;
