namespace SmartQr.Api.Application.Codes.Core.Validation;

/// <summary>Shared predicates for the code validators — kept in one place so create, update, and rule validation agree.</summary>
internal static class CodeValidationRules
{
    /// <summary>True when the value is an absolute http/https URL (the redirect hot path 302s to it verbatim).</summary>
    public static bool IsAbsoluteHttpUrl(string? value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
