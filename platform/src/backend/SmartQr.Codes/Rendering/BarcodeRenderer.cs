using SmartQr.Codes.Models;
using SmartQr.Common.Domain.Codes.Enums;
using ZXing.Common;

namespace SmartQr.Codes.Rendering;

/// <summary>ZXing.Net-backed barcode renderer. Emits SVG (vector, managed — no native deps).</summary>
public sealed class BarcodeRenderer : IBarcodeRenderer
{
    /// <inheritdoc />
    public string RenderSvg(string payload, BarcodeFormat format, CodeRenderOptions options)
    {
        var writer = new ZXing.BarcodeWriterSvg
        {
            Format = MapFormat(format),
            Options = new EncodingOptions
            {
                Width = 300,
                Height = IsOneDimensional(format) ? 120 : 300,
                Margin = 4,
                PureBarcode = false,
            },
        };

        return writer.Write(payload).ToString();
    }

    private static ZXing.BarcodeFormat MapFormat(BarcodeFormat format) => format switch
    {
        BarcodeFormat.QrCode => ZXing.BarcodeFormat.QR_CODE,
        BarcodeFormat.DataMatrix => ZXing.BarcodeFormat.DATA_MATRIX,
        BarcodeFormat.Pdf417 => ZXing.BarcodeFormat.PDF_417,
        BarcodeFormat.Aztec => ZXing.BarcodeFormat.AZTEC,
        BarcodeFormat.Code128 => ZXing.BarcodeFormat.CODE_128,
        BarcodeFormat.Ean13 => ZXing.BarcodeFormat.EAN_13,
        BarcodeFormat.UpcA => ZXing.BarcodeFormat.UPC_A,
        _ => ZXing.BarcodeFormat.CODE_128,
    };

    private static bool IsOneDimensional(BarcodeFormat format) => format
        is BarcodeFormat.Code128 or BarcodeFormat.Ean13 or BarcodeFormat.UpcA;
}
