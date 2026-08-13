namespace SmartQr.Application.Codes.Validators;

/// <summary>Shared predicates for the create, update, and rule validators.</summary>
internal static class CodeValidationRules
{
    /// <summary>True when the value is an absolute http/https URL.</summary>
    public static bool IsAbsoluteHttpUrl(string? value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
