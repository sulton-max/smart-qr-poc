namespace SmartQr.Domain.Codes.Core.Enums;

/// <summary>Defines the symbology used to render a code.</summary>
public enum BarcodeFormat
{
    /// <summary>Represents a QR code — 2D.</summary>
    QrCode,

    /// <summary>Represents a Data Matrix — 2D.</summary>
    DataMatrix,

    /// <summary>Represents a PDF417 — 2D stacked.</summary>
    Pdf417,

    /// <summary>Represents an Aztec code — 2D.</summary>
    Aztec,

    /// <summary>Represents a Code 128 — 1D.</summary>
    Code128,

    /// <summary>Represents an EAN-13 — 1D retail product barcode.</summary>
    Ean13,

    /// <summary>Represents a UPC-A — 1D retail product barcode.</summary>
    UpcA,
}
