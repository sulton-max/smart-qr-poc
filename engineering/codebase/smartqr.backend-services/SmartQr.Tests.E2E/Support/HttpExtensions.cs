namespace SmartQr.Tests.E2E.Support;

// The JSON request/response plumbing (AsJson / PostJsonAsync / PutJsonAsync / PatchJsonAsync /
// ReadEnvelopeAsync) lives in the SDK testing package — WoW.Two.Sdk.Backend.Beta.Testing.Web.HttpExtensions.

/// <summary>Builders for the JSON request bodies the codes endpoints accept.</summary>
public static class CodeRequests
{
    /// <summary>A plain url create/update body — carries the url content plus <paramref name="rules"/> and a trailing Default catch-all to <paramref name="destination"/>. The catch-all is a rule now (the fallback column is retired), so the code still resolves for every scan.</summary>
    public static object Code(string name, string destination, IEnumerable<object>? rules = null) => new
    {
        name,
        codeType = "Qr",
        barcodeFormat = "QrCode",
        content = new { type = "url", url = destination },
        rules = (rules ?? []).Append(DefaultRule(destination)).ToArray(),
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

    /// <summary>A Default catch-all rule — matches any scan; ordered last so specific rules win. Replaces the retired fallback URL.</summary>
    public static object DefaultRule(string destination, int order = 99)
        => Rule(order, "Default", null, destination);

    /// <summary>A create/update body carrying typed <paramref name="content"/> (e.g. <c>new { type = "wifi", ssid = "…" }</c>) — no rules; the backend derives the payload from the content.</summary>
    public static object Content(string name, object content) => new
    {
        name,
        codeType = "Qr",
        barcodeFormat = "QrCode",
        rules = Array.Empty<object>(),
        content,
    };

    /// <summary>A mobile-app-link create/update body — sends the typed store links + fallback choice; the backend derives the device rules plus an optional Default catch-all (only when a fallback store is chosen).</summary>
    public static object MobileApp(string name, string? appStore = null, string? playStore = null, string? other = null, string? fallback = null) => new
    {
        name,
        codeType = "Qr",
        barcodeFormat = "QrCode",
        rules = Array.Empty<object>(),
        content = new { type = "mobileApp", appStore, playStore, other, fallback },
    };
}
