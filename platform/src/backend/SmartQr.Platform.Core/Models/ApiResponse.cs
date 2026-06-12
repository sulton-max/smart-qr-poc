using System.Net;

namespace SmartQr.Common.Models;

/// <summary>Base API response envelope.</summary>
public record ApiResponse
{
    public const string UnexpectedErrorMessage = "Unexpected error";
}

/// <summary>Result-pattern API response — Success carries typed data, Failure carries error + status code.</summary>
/// <remarks>
/// Controllers: use <see cref="Ok"/> to wrap successful data (failures use ProblemDetails).
/// API clients: return Success or Failure based on HTTP status — callers pattern-match instead of catching exceptions.
/// </remarks>
public abstract record ApiResponse<T> : ApiResponse
{
    private ApiResponse() { }

    /// <summary>Successful response with typed payload.</summary>
    public sealed record Success : ApiResponse<T>
    {
        /// <summary>The response payload.</summary>
        public required T Data { get; init; }
    }

    /// <summary>Failed response — deserialized from non-success HTTP status codes.</summary>
    public sealed record Failure : ApiResponse<T>
    {
        /// <summary>HTTP status code from the response.</summary>
        public required HttpStatusCode StatusCode { get; init; }

        /// <summary>Error description (from ProblemDetails.Detail, response body, or status reason).</summary>
        public required string Error { get; init; }
    }

    /// <summary>Creates a success response — used by controllers to wrap data in the envelope.</summary>
    public static Success Ok(T data) => new() { Data = data };
}
