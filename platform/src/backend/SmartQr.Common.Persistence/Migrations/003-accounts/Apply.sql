-- ============================================================
-- 003-accounts — registered user accounts (Google OAuth)
-- Table: users
-- A registered account layered over the guest-first identity. The id is the durable user key:
-- on same-device sign-up the guest cookie Guid becomes this id (guest codes already point at it),
-- and cross-device sign-up merges the guest's codes onto it. No FK from codes.user_id — a guest
-- owner has no users row, so ownership stays a bare Guid.
-- Mirrors the EF Core model: snake_case columns, enums-as-text n/a, timestamptz.
-- EF is a pure mapper over this hand-authored schema (schema-first).
-- ============================================================

CREATE TABLE users (
    id             uuid        NOT NULL,
    google_subject text        NOT NULL,
    email          text        NOT NULL,
    name           text        NOT NULL,
    avatar_url     text        NULL,
    created_at     timestamptz NOT NULL,
    updated_at     timestamptz NOT NULL,
    CONSTRAINT pk_users PRIMARY KEY (id)
);

-- Google's subject (`sub`) is the stable per-account identifier — the find-or-create key on sign-in.
CREATE UNIQUE INDEX ix_users_google_subject ON users (google_subject);
