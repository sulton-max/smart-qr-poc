using SmartQr.Application.Codes.Content.MobileApp;
using SmartQr.Domain.Codes.Core.Enums;

namespace SmartQr.Application.Codes.Content;

/// <summary>
/// Registry of content-type specs — resolves the strategy that owns a content type's validation and routing projection.
/// Specs are stateless singletons, keyed by <see cref="CodeContentType"/>; extend by registering one per content type.
/// Types with no spec (url / static / legacy) fall through to the generic fallback-URL path.
/// </summary>
public static class ContentTypes
{
    private static readonly IReadOnlyDictionary<CodeContentType, IContentTypeSpec> Specs =
        new IContentTypeSpec[]
        {
            new MobileAppLinkContentSpec(),
        }.ToDictionary(spec => spec.Type);

    /// <summary>Resolves the spec that owns a content type, or null when there is no content type or the generic path applies.</summary>
    public static IContentTypeSpec? Resolve(CodeContentType? type) =>
        type is { } value && Specs.TryGetValue(value, out var spec) ? spec : null;
}
