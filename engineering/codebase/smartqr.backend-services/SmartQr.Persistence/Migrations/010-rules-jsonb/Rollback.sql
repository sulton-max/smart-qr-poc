-- ============================================================
-- 010-rules-jsonb — Rollback: restore the routing_rules table and the code-level content column.
-- Structure only; the rules held in the jsonb document are not projected back into rows.
-- ============================================================

ALTER TABLE scan_events DROP COLUMN matched_rule_order;
ALTER TABLE scan_events ADD COLUMN matched_rule_id uuid NULL;

ALTER TABLE codes ALTER COLUMN slug SET NOT NULL;

ALTER TABLE codes DROP COLUMN content_type;
ALTER TABLE codes DROP COLUMN mode;
ALTER TABLE codes DROP COLUMN rules;

ALTER TABLE codes ADD COLUMN content_json jsonb NOT NULL DEFAULT '{"type":"url","url":""}'::jsonb;
ALTER TABLE codes ALTER COLUMN content_json DROP DEFAULT;

CREATE TABLE routing_rules (
    id               uuid        NOT NULL,
    code_id          uuid        NOT NULL,
    "order"          integer     NOT NULL,
    condition_type   text        NOT NULL,
    condition_value  text        NULL,
    destination      text        NOT NULL,
    created_at       timestamptz NOT NULL,
    CONSTRAINT pk_routing_rules PRIMARY KEY (id),
    CONSTRAINT fk_routing_rules_codes FOREIGN KEY (code_id) REFERENCES codes (id) ON DELETE CASCADE
);

CREATE INDEX ix_routing_rules_code_id_order ON routing_rules (code_id, "order");
