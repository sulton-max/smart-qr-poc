namespace SmartQr.Common.Domain.Codes.Enums;

/// <summary>High-level kind of scannable/clickable artifact a record represents.</summary>
public enum CodeType
{
    /// <summary>A QR code (2D) fronting a dynamic redirect.</summary>
    Qr,

    /// <summary>A 1D/2D barcode (see <see cref="BarcodeFormat"/>) fronting a dynamic redirect.</summary>
    Barcode,

    /// <summary>A bare short link — same redirect engine, no rendered code.</summary>
    Link,
}
