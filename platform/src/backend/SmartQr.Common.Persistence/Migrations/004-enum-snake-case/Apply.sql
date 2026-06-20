-- ============================================================
-- 004-enum-snake-case — re-encode enum text columns C# member name → snake_case (e.g. 'QrCode' → 'qr_code').
-- Storage only; the JSON wire format still emits the C# member name. No-op on a fresh DB, required where rows exist.
-- Value-scoped UPDATEs — transaction-safe and re-runnable after a partial apply.
-- ============================================================

-- ── codes.code_type (CodeType) ──
UPDATE codes SET code_type = 'qr'      WHERE code_type = 'Qr';
UPDATE codes SET code_type = 'barcode' WHERE code_type = 'Barcode';
UPDATE codes SET code_type = 'link'    WHERE code_type = 'Link';

-- ── codes.barcode_format (BarcodeFormat) ──
UPDATE codes SET barcode_format = 'qr_code'     WHERE barcode_format = 'QrCode';
UPDATE codes SET barcode_format = 'data_matrix' WHERE barcode_format = 'DataMatrix';
UPDATE codes SET barcode_format = 'pdf_417'     WHERE barcode_format = 'Pdf417';
UPDATE codes SET barcode_format = 'aztec'       WHERE barcode_format = 'Aztec';
UPDATE codes SET barcode_format = 'code_128'    WHERE barcode_format = 'Code128';
UPDATE codes SET barcode_format = 'ean_13'      WHERE barcode_format = 'Ean13';
UPDATE codes SET barcode_format = 'upc_a'       WHERE barcode_format = 'UpcA';

-- ── routing_rules.condition_type (RuleConditionType) ──
UPDATE routing_rules SET condition_type = 'device'      WHERE condition_type = 'Device';
UPDATE routing_rules SET condition_type = 'country'     WHERE condition_type = 'Country';
UPDATE routing_rules SET condition_type = 'language'    WHERE condition_type = 'Language';
UPDATE routing_rules SET condition_type = 'time_of_day' WHERE condition_type = 'TimeOfDay';
UPDATE routing_rules SET condition_type = 'default'     WHERE condition_type = 'Default';

-- ── scan_events.device (DeviceType) ──
UPDATE scan_events SET device = 'unknown' WHERE device = 'Unknown';
UPDATE scan_events SET device = 'ios'     WHERE device = 'Ios';
UPDATE scan_events SET device = 'android' WHERE device = 'Android';
UPDATE scan_events SET device = 'desktop' WHERE device = 'Desktop';
UPDATE scan_events SET device = 'bot'     WHERE device = 'Bot';

-- ── subscriptions.plan (Plan) ──
UPDATE subscriptions SET plan = 'free'   WHERE plan = 'Free';
UPDATE subscriptions SET plan = 'solo'   WHERE plan = 'Solo';
UPDATE subscriptions SET plan = 'pro'    WHERE plan = 'Pro';
UPDATE subscriptions SET plan = 'agency' WHERE plan = 'Agency';

-- ── subscriptions.status (SubscriptionStatus) ──
UPDATE subscriptions SET status = 'active'     WHERE status = 'Active';
UPDATE subscriptions SET status = 'trialing'   WHERE status = 'Trialing';
UPDATE subscriptions SET status = 'past_due'   WHERE status = 'PastDue';
UPDATE subscriptions SET status = 'canceled'   WHERE status = 'Canceled';
UPDATE subscriptions SET status = 'incomplete' WHERE status = 'Incomplete';
UPDATE subscriptions SET status = 'unpaid'     WHERE status = 'Unpaid';
