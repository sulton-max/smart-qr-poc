-- ============================================================
-- 002-billing — Stripe subscriptions
-- Table: subscriptions
-- Mirrors the EF Core model: snake_case columns, enums-as-text, timestamptz.
-- EF is a pure mapper over this hand-authored schema (schema-first).
-- ============================================================

CREATE TABLE subscriptions (
    id                     uuid        NOT NULL,
    user_id                uuid        NOT NULL,
    plan                   text        NOT NULL,
    status                 text        NOT NULL,
    stripe_customer_id     text        NOT NULL,
    stripe_subscription_id text        NOT NULL,
    current_period_end     timestamptz NULL,
    created_at             timestamptz NOT NULL,
    updated_at             timestamptz NOT NULL,
    CONSTRAINT pk_subscriptions PRIMARY KEY (id)
);

-- One live subscription per user — the lookup key and Stripe Checkout client_reference_id.
CREATE UNIQUE INDEX ix_subscriptions_user_id ON subscriptions (user_id);
