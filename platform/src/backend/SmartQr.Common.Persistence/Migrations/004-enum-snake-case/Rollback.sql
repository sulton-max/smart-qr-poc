-- ============================================================
-- 004-enum-snake-case — Rollback: exact reverse of Apply (snake_case → C# member name), for reverting to a C#-name-reading binary.
-- ============================================================

-- ── codes.code_type (CodeType) ──
UPDATE codes SET code_type = 'Qr'      WHERE code_type = 'qr';
UPDATE codes SET code_type = 'Barcode' WHERE code_type = 'barcode';
UPDATE codes SET code_type = 'Link'    WHERE code_type = 'link';

-- ── codes.barcode_format (BarcodeFormat) ──
UPDATE codes SET barcode_format = 'QrCode'     WHERE barcode_format = 'qr_code';
UPDATE codes SET barcode_format = 'DataMatrix' WHERE barcode_format = 'data_matrix';
UPDATE codes SET barcode_format = 'Pdf417'     WHERE barcode_format = 'pdf_417';
UPDATE codes SET barcode_format = 'Aztec'      WHERE barcode_format = 'aztec';
UPDATE codes SET barcode_format = 'Code128'    WHERE barcode_format = 'code_128';
UPDATE codes SET barcode_format = 'Ean13'      WHERE barcode_format = 'ean_13';
UPDATE codes SET barcode_format = 'UpcA'       WHERE barcode_format = 'upc_a';

-- ── routing_rules.condition_type (RuleConditionType) ──
UPDATE routing_rules SET condition_type = 'Device'    WHERE condition_type = 'device';
UPDATE routing_rules SET condition_type = 'Country'   WHERE condition_type = 'country';
UPDATE routing_rules SET condition_type = 'Language'  WHERE condition_type = 'language';
UPDATE routing_rules SET condition_type = 'TimeOfDay' WHERE condition_type = 'time_of_day';
UPDATE routing_rules SET condition_type = 'Default'   WHERE condition_type = 'default';

-- ── scan_events.device (DeviceType) ──
UPDATE scan_events SET device = 'Unknown' WHERE device = 'unknown';
UPDATE scan_events SET device = 'Ios'     WHERE device = 'ios';
UPDATE scan_events SET device = 'Android' WHERE device = 'android';
UPDATE scan_events SET device = 'Desktop' WHERE device = 'desktop';
UPDATE scan_events SET device = 'Bot'     WHERE device = 'bot';

-- ── subscriptions.plan (Plan) ──
UPDATE subscriptions SET plan = 'Free'   WHERE plan = 'free';
UPDATE subscriptions SET plan = 'Solo'   WHERE plan = 'solo';
UPDATE subscriptions SET plan = 'Pro'    WHERE plan = 'pro';
UPDATE subscriptions SET plan = 'Agency' WHERE plan = 'agency';

-- ── subscriptions.status (SubscriptionStatus) ──
UPDATE subscriptions SET status = 'Active'     WHERE status = 'active';
UPDATE subscriptions SET status = 'Trialing'   WHERE status = 'trialing';
UPDATE subscriptions SET status = 'PastDue'    WHERE status = 'past_due';
UPDATE subscriptions SET status = 'Canceled'   WHERE status = 'canceled';
UPDATE subscriptions SET status = 'Incomplete' WHERE status = 'incomplete';
UPDATE subscriptions SET status = 'Unpaid'     WHERE status = 'unpaid';
