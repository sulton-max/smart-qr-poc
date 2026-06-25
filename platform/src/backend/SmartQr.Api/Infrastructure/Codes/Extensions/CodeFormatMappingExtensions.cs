using DomainBarcodeFormat = SmartQr.Common.Domain.Codes.Enums.BarcodeFormat;
using RenderBarcodeFormat = WoW.Two.Sdk.Backend.Beta.Codes.Models.BarcodeFormat;

namespace SmartQr.Api.Infrastructure.Codes.Extensions;

/// <summary>
/// Maps the persisted domain symbology enum to the SDK render engine's symbology vocabulary at the render boundary.
/// The product owns the persisted <see cref="DomainBarcodeFormat"/> (a <c>codes</c> column); the SDK owns the render
/// <see cref="RenderBarcodeFormat"/>. Members are 1:1 by name — the switch fails to compile if either side diverges.
/// </summary>
internal static class CodeFormatMappingExtensions
{
    /// <summary>Converts the persisted domain barcode format to the SDK render barcode format.</summary>
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
