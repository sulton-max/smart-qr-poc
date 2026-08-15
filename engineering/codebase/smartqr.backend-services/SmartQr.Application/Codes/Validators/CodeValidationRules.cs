namespace SmartQr.Application.Codes.Validators;

/// <summary>Contains the shared validation predicates.</summary>
internal static class CodeValidationRules
{
    /// <summary>Returns whether the value is an absolute http or https URL.</summary>
    public static bool IsAbsoluteHttpUrl(string? value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
