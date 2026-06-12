-- ============================================================
-- 001-baseline — Smart QR initial schema
-- Tables: codes, routing_rules, scan_events
-- Mirrors the EF Core model: snake_case columns, enums-as-text, jsonb, timestamptz.
-- EF is a pure mapper over this hand-authored schema (schema-first).
-- ============================================================

CREATE TABLE codes (
    id              uuid        NOT NULL,
    slug            text        NOT NULL,
    user_id         uuid        NOT NULL,
    name            text        NOT NULL,
    code_type       text        NOT NULL,
    barcode_format  text        NOT NULL,
    fallback_url    text        NOT NULL,
    is_active       boolean     NOT NULL,
    never_expires   boolean     NOT NULL,
    expires_at      timestamptz NULL,
    max_scans       bigint      NULL,
    password_hash   text        NULL,
    style_json      jsonb       NULL,
    scan_count      bigint      NOT NULL,
    created_at      timestamptz NOT NULL,
    updated_at      timestamptz NOT NULL,
    CONSTRAINT pk_codes PRIMARY KEY (id)
);

-- Slug is the immutable public id encoded into the printed code — unique + the redirect lookup key.
CREATE UNIQUE INDEX ix_codes_slug ON codes (slug);

CREATE TABLE routing_rules (
    id               uuid        NOT NULL,
    code_id          uuid        NOT NULL,
    "order"          integer     NOT NULL,
    condition_type   text        NOT NULL,
    condition_value  text        NULL,
    destination      text        NOT NULL,
    created_at       timestamptz NOT NULL,
    CONSTRAINT pk_routing_rules PRIMARY KEY (id),
    CONSTRAINT fk_routing_rules_codes_code_id FOREIGN KEY (code_id)
        REFERENCES codes (id) ON DELETE CASCADE
);

-- Rules are read in evaluation order for a code.
CREATE INDEX ix_routing_rules_code_id_order ON routing_rules (code_id, "order");

CREATE TABLE scan_events (
    id                uuid        NOT NULL,
    code_id           uuid        NOT NULL,
    scanned_at        timestamptz NOT NULL,
    device            text        NOT NULL,
    country_code      text        NULL,
    os                text        NULL,
    referrer          text        NULL,
    user_agent_hash   text        NULL,
    matched_rule_id   uuid        NULL,
    destination_url   text        NOT NULL,
    CONSTRAINT pk_scan_events PRIMARY KEY (id)
);

-- Time-series reads per code (dashboard charts). Append-only, written off the redirect hot path.
CREATE INDEX ix_scan_events_code_id_scanned_at ON scan_events (code_id, scanned_at);
