using DomainBarcodeFormat = SmartQr.Domain.Codes.Core.Enums.BarcodeFormat;
using RenderBarcodeFormat = WoW.Two.Sdk.Backend.Beta.Codes.Models.BarcodeFormat;

namespace SmartQr.Infrastructure.Codes.Core.Extensions;

/// <summary>Extends <see cref="DomainBarcodeFormat"/> for SDK render mapping.</summary>
public static class CodeFormatMappingExtensions
{
    /// <summary>Converts the domain barcode format to the SDK render format.</summary>
    public static RenderBarcodeFormat ToRender(this DomainBarcodeFormat format) => format switch
    {
        DomainBarcodeFormat.QrCode => RenderBarcodeFormat.QrCode,
        DomainBarcodeFormat.DataMatrix => RenderBarcodeFormat.DataMatrix,
        DomainBarcodeFormat.Pdf417 => RenderBarcodeFormat.Pdf417,
        DomainBarcodeFormat.Aztec => RenderBarcodeFormat.Aztec,
        DomainBarcodeFormat.Code128 => RenderBarcodeFormat.Code128,
        DomainBarcodeFormat.Ean13 => RenderBarcodeFormat.Ean13,
        DomainBarcodeFormat.UpcA => RenderBarcodeFormat.UpcA,
        _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unknown barcode format."),
    };
}
