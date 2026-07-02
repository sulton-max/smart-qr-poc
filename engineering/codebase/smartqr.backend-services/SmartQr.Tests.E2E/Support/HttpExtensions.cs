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

    /// <summary>A static create/update body — bakes <paramref name="payload"/> into the symbol (no redirect), with an empty fallback and no rules.</summary>
    /// <param name="fields">The raw field values persisted to round-trip the builder form; pass <c>null</c> for none.</param>
    public static object StaticCode(string name, string type, string payload, object? fields = null) => new
    {
        name,
        codeType = "Qr",
        barcodeFormat = "QrCode",
        fallbackUrl = "",
        rules = Array.Empty<object>(),
        content = new { type, fields = fields ?? new { }, payload },
    };

    /// <summary>A mobile-app-link create/update body — sends only the raw store links + fallback choice; the backend derives the device rules + fallback URL.</summary>
    public static object MobileApp(string name, string? ios = null, string? android = null, string? other = null, string? fallback = null)
    {
        var fields = new Dictionary<string, string>();
        if (ios is not null) fields["ios"] = ios;
        if (android is not null) fields["android"] = android;
        if (other is not null) fields["other"] = other;
        if (fallback is not null) fields["fallback"] = fallback;
        return new
        {
            name,
            codeType = "Qr",
            barcodeFormat = "QrCode",
            fallbackUrl = "",
            rules = Array.Empty<object>(),
            content = new { type = "mobileApp", fields, payload = (string?)null },
        };
    }
}
