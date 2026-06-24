using QRCoder;

namespace SmartQr.Tests.Unit;

/// <summary>
/// The retired QRCoder <c>SvgQRCode</c> output, computed live — the golden the custom emitter's default spec must match
/// byte-for-byte. Kept as a test-only oracle so the byte-parity gate compares against the real prior behavior, not a snapshot.
/// </summary>
internal static class QrCoderReference
{
    /// <summary>Renders <paramref name="payload"/> exactly as the pre-v0.5 path did: <c>SvgQRCode.GetGraphic(20, "#000000", "#FFFFFF")</c> at ECC Q.</summary>
    public static string Svg(string payload)
    {
        using var generator = new QRCodeGenerator();
        var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        return new SvgQRCode(data).GetGraphic(20, "#000000", "#FFFFFF");
    }
}
