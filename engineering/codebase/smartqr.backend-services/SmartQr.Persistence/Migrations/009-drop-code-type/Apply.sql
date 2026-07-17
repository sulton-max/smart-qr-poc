-- ============================================================
-- 009-drop-code-type — retire the code_type column.
-- Redundant: QR *is* BarcodeFormat.QrCode, so Qr-vs-Barcode already lives in barcode_format, and
-- Link was never a symbology (image-less codes aren't offered — a forwarder is url content rendered
-- as a QR). barcode_format is the sole symbology; nothing derives from code_type. The 004 re-encode
-- of this column is moot. No data migration — no value is preserved.
-- ============================================================

ALTER TABLE codes DROP COLUMN code_type;
