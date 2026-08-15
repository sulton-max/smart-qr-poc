using QRCoder;

namespace SmartQr.Tests.Unit;

/// <summary>Provides live-computed QRCoder <c>SvgQRCode</c> output as a parity reference.</summary>
internal static class QrCoderReference
{
    /// <summary>Renders <paramref name="payload"/> at ECC Q.</summary>
    public static string Svg(string payload)
    {
        using var generator = new QRCodeGenerator();
        var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        return new SvgQRCode(data).GetGraphic(20, "#000000", "#FFFFFF");
    }
}
