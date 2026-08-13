using QRCoder;

namespace SmartQr.Tests.Unit;

/// <summary>Live-computed QRCoder <c>SvgQRCode</c> output — the default spec must match it byte-for-byte.</summary>
internal static class QrCoderReference
{
    /// <summary>Renders <paramref name="payload"/> exactly as the pre-v0.5 path did, at ECC Q.</summary>
    public static string Svg(string payload)
    {
        using var generator = new QRCodeGenerator();
        var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        return new SvgQRCode(data).GetGraphic(20, "#000000", "#FFFFFF");
    }
}
