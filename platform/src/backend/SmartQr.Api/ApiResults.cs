using SmartQr.Common.Domain.Results;

namespace SmartQr.Api;

/// <summary>Maps a typed application failure to an HTTP status code — the single app-side error→status map.</summary>
internal static class ApiResults
{
    /// <summary>Translates a <see cref="FailureCategory"/> into an HTTP status code — the pure category→status map.</summary>
    public static int ToStatusCode(FailureCategory category) => category switch
    {
        FailureCategory.Validation => StatusCodes.Status400BadRequest,
        FailureCategory.NotFound => StatusCodes.Status404NotFound,
        FailureCategory.Conflict => StatusCodes.Status409Conflict,
        FailureCategory.Unauthorized => StatusCodes.Status401Unauthorized,
        FailureCategory.Forbidden => StatusCodes.Status403Forbidden,
        FailureCategory.PaymentRequired => StatusCodes.Status402PaymentRequired,
        FailureCategory.Unexpected => StatusCodes.Status500InternalServerError,
        _ => StatusCodes.Status500InternalServerError
    };
}
