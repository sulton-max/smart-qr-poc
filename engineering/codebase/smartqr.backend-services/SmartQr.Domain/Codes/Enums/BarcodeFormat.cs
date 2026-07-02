namespace SmartQr.Domain.Codes.Enums;

/// <summary>Defines the concrete symbology used to render a code — QR by default; the rest cover the "all code types" surface.</summary>
public enum BarcodeFormat
{
    /// <summary>Represents a QR code — 2D, the default.</summary>
    QrCode,

    /// <summary>Represents a Data Matrix — 2D, industrial / pharma / small-part marking, GS1.</summary>
    DataMatrix,

    /// <summary>Represents a PDF417 — 2D stacked, IDs / tickets / boarding passes.</summary>
    Pdf417,

    /// <summary>Represents an Aztec code — 2D, transport tickets (robust at low resolution, no quiet zone).</summary>
    Aztec,

    /// <summary>Represents a Code 128 — 1D, logistics / asset labels.</summary>
    Code128,

    /// <summary>Represents an EAN-13 — 1D retail product barcode.</summary>
    Ean13,

    /// <summary>Represents a UPC-A — 1D retail product barcode.</summary>
    UpcA,
}
