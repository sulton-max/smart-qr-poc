namespace SmartQr.Tests.E2E.Support;

// The JSON request/response plumbing (AsJson / PostJsonAsync / PutJsonAsync / PatchJsonAsync /
// ReadEnvelopeAsync) lives in the SDK testing package — WoW.Two.Sdk.Backend.Beta.Testing.Web.HttpExtensions.

/// <summary>Builders for the JSON request bodies the codes endpoints accept.</summary>
public static class CodeRequests
{
    /// <summary>A create/update body. <paramref name="rules"/> is the ordered rule set; pass <c>[]</c> for none.</summary>
    public static object Code(string name, string fallbackUrl, IEnumerable<object>? rules = null) => new
    {
        name,
        codeType = "Qr",
        barcodeFormat = "QrCode",
        fallbackUrl,
        rules = rules?.ToArray() ?? [],
    };

    /// <summary>A single routing rule body.</summary>
    public static object Rule(int order, string conditionType, string? conditionValue, string destination) => new
    {
        order,
        conditionType,
        conditionValue,
        destination,
    };

    /// <summary>An iOS device rule (matches <c>DeviceType.Ios</c>).</summary>
    public static object IosRule(string destination, int order = 1)
        => Rule(order, "Device", "Ios", destination);

    /// <summary>A create/update body carrying typed <paramref name="content"/> (e.g. <c>new { type = "wifi", ssid = "…" }</c>) — empty fallback + no rules; the backend derives the payload from the content.</summary>
    public static object Content(string name, object content) => new
    {
        name,
        codeType = "Qr",
        barcodeFormat = "QrCode",
        fallbackUrl = "",
        rules = Array.Empty<object>(),
        content,
    };

    /// <summary>A mobile-app-link create/update body — sends the typed store links + fallback choice; the backend derives the device rules + fallback URL.</summary>
    public static object MobileApp(string name, string? appStore = null, string? playStore = null, string? other = null, string? fallback = null) => new
    {
        name,
        codeType = "Qr",
        barcodeFormat = "QrCode",
        fallbackUrl = "",
        rules = Array.Empty<object>(),
        content = new { type = "mobileApp", appStore, playStore, other, fallback },
    };
}
