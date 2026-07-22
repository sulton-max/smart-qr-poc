namespace SmartQr.Tests.E2E.Support;

// The JSON request/response plumbing (AsJson / PostJsonAsync / PutJsonAsync / PatchJsonAsync /
// ReadEnvelopeAsync) lives in the SDK testing package — WoW.Two.Sdk.Backend.Beta.Testing.Web.HttpExtensions.

/// <summary>Builders for the JSON request bodies the codes endpoints accept — every rule carries the content it serves.</summary>
public static class CodeRequests
{
    /// <summary>A static url create body — one default rule carrying the url content. Static content bakes its payload; the code never reaches the redirect.</summary>
    public static object StaticUrl(string name, string destination) => new
    {
        name,
        barcodeFormat = "QrCode",
        mode = "static",
        contentType = "url",
        rules = new object[] { DefaultRule(new { type = "url", url = destination }) },
    };

    /// <summary>A dynamic url create body — the given conditional rules plus a default catch-all, each carrying url content.</summary>
    public static object DynamicUrl(string name, string destination, IEnumerable<object>? rules = null) => new
    {
        name,
        barcodeFormat = "QrCode",
        mode = "dynamic",
        contentType = "url",
        rules = (rules ?? []).Append(DefaultRule(new { type = "url", url = destination })).ToArray(),
    };

    /// <summary>A create/update body carrying one default rule with typed <paramref name="content"/> (e.g. <c>new { type = "wifi", ssid = "…" }</c>).</summary>
    public static object Static(string name, string contentType, object content) => new
    {
        name,
        barcodeFormat = "QrCode",
        mode = "static",
        contentType,
        rules = new object[] { DefaultRule(content) },
    };

    /// <summary>A conditional routing rule carrying the content it serves.</summary>
    public static object ConditionalRule(string condition, string conditionValue, object content, int order = 1) => new
    {
        type = "conditional",
        order,
        condition,
        conditionValue,
        content,
    };

    /// <summary>An iOS device rule (matches <c>DeviceType.Ios</c>), carrying url content.</summary>
    public static object IosRule(string destination, int order = 1)
        => ConditionalRule("Device", "Ios", new { type = "url", url = destination }, order);

    /// <summary>The default catch-all rule, carrying the content served when no conditional rule matches.</summary>
    public static object DefaultRule(object content) => new { type = "default", content };

    /// <summary>The default-pointer rule, nominating an existing rule's content by its order.</summary>
    public static object DefaultPointerRule(int targetOrder) => new { type = "defaultPointer", targetOrder };
}
