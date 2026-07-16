-- ============================================================
-- 007-drop-fallback-url — retire the standalone fallback_url column.
-- The catch-all is now an optional Default routing rule (RuleConditionType.Default), and every
-- code carries a required typed content. A scan that matches no rule and has no Default rule is
-- deliberately NotFound (restrictive routing). No data migration — content + rules already carry
-- every destination.
-- ============================================================

ALTER TABLE codes DROP COLUMN fallback_url;
