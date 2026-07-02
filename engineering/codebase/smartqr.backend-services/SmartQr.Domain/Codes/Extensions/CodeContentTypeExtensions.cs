using SmartQr.Domain.Codes.Enums;

namespace SmartQr.Domain.Codes.Extensions;

/// <summary>Provides support checks for <see cref="CodeContentType"/> — which content types the builder can currently create versus those that are known but not yet buildable.</summary>
public static class CodeContentTypeExtensions
{
    /// <summary>The content types the builder currently supports; every other <see cref="CodeContentType"/> value is known but not yet buildable.</summary>
    public static readonly IReadOnlySet<CodeContentType> SupportedCodeTypes = new HashSet<CodeContentType>
    {
        CodeContentType.Url,
        CodeContentType.MobileApp,
        CodeContentType.Text,
        CodeContentType.Email,
        CodeContentType.Sms,
        CodeContentType.Phone,
        CodeContentType.Geo,
        CodeContentType.Wifi,
        CodeContentType.VCard,
        CodeContentType.Calendar,
    };

    /// <summary>Determines whether the content type is one the builder can currently create.</summary>
    public static bool IsSupported(this CodeContentType contentType) => SupportedCodeTypes.Contains(contentType);
}
