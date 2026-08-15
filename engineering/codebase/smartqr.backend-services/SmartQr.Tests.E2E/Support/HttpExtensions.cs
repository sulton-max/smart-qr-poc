namespace SmartQr.Tests.E2E.Support;

// The JSON request/response plumbing (AsJson / PostJsonAsync / PutJsonAsync / PatchJsonAsync /
// ReadEnvelopeAsync) lives in the SDK testing package — WoW.Two.Sdk.Backend.Beta.Testing.Web.HttpExtensions.

/// <summary>Provides builders for the JSON request bodies the codes endpoints accept.</summary>
public static class CodeRequests
{
    /// <summary>Builds the full style block every create and update body carries.</summary>
    public static object Style() => new
    {
        foregroundColor = "#000000",
        backgroundColor = "#FFFFFF",
        transparentBackground = false,
        eccLevel = "Q",
        quietZoneModules = 4,
        moduleShape = "square",
        finderShape = "square",
        finderDotShape = "square",
    };

    /// <summary>Builds a static url create body with one default rule carrying the url content.</summary>
    public static object StaticUrl(string name, string destination) => new
    {
        name,
        barcodeFormat = "QrCode",
        mode = "static",
        contentType = "url",
        rules = new object[] { DefaultRule(new { type = "url", url = destination }) },
        style = Style(),
    };

    /// <summary>Builds a dynamic url create body from the given rules plus a url catch-all.</summary>
    public static object DynamicUrl(string name, string destination, IEnumerable<object>? rules = null) => new
    {
        name,
        barcodeFormat = "QrCode",
        mode = "dynamic",
        contentType = "url",
        rules = (rules ?? []).Append(DefaultRule(new { type = "url", url = destination })).ToArray(),
        style = Style(),
    };

    /// <summary>Builds a create or update body with one default rule carrying <paramref name="content"/>.</summary>
    public static object Static(string name, string contentType, object content) => new
    {
        name,
        barcodeFormat = "QrCode",
        mode = "static",
        contentType,
        rules = new object[] { DefaultRule(content) },
        style = Style(),
    };

    /// <summary>Builds a conditional routing rule carrying the content it serves.</summary>
    public static object ConditionalRule(string condition, string conditionValue, object content, int order = 1) => new
    {
        type = "conditional",
        order,
        condition,
        conditionValue,
        content,
    };

    /// <summary>Builds an iOS device rule carrying url content.</summary>
    public static object IosRule(string destination, int order = 1)
        => ConditionalRule("Device", "Ios", new { type = "url", url = destination }, order);

    /// <summary>Builds the default catch-all rule carrying the content it serves.</summary>
    public static object DefaultRule(object content) => new { type = "default", content };

    /// <summary>Builds the default-pointer rule that nominates an existing rule's content by order.</summary>
    public static object DefaultPointerRule(int targetOrder) => new { type = "defaultPointer", targetOrder };
}
