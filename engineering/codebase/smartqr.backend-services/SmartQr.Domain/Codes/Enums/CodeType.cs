namespace SmartQr.Domain.Codes.Enums;

/// <summary>Defines the high-level kind of scannable/clickable artifact a code represents.</summary>
public enum CodeType
{
    /// <summary>Represents a QR code (2D) fronting a dynamic redirect.</summary>
    Qr,

    /// <summary>Represents a 1D/2D barcode (see <see cref="BarcodeFormat"/>) fronting a dynamic redirect.</summary>
    Barcode,

    /// <summary>Represents a bare short link — same redirect engine, no rendered code.</summary>
    Link,
}
