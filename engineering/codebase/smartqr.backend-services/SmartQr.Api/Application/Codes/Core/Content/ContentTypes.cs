namespace SmartQr.Api.Application.Codes.Core.Content;

/// <summary>
/// Registry of content-type specs — resolves the strategy that owns a content type's validation and routing projection.
/// Specs are stateless singletons; extend by registering one per content type. Types with no spec (url / static / legacy)
/// fall through to the generic fallback-URL path.
/// </summary>
public static class ContentTypes
{
    private static readonly IReadOnlyDictionary<string, IContentTypeSpec> Specs =
        new IContentTypeSpec[]
        {
            new MobileAppLinkContentSpec(),
        }.ToDictionary(spec => spec.Type, StringComparer.OrdinalIgnoreCase);

    /// <summary>Resolves the spec that owns a content-type id, or null when the generic path applies.</summary>
    public static IContentTypeSpec? Resolve(string? type) =>
        type is not null && Specs.TryGetValue(type, out var spec) ? spec : null;
}
