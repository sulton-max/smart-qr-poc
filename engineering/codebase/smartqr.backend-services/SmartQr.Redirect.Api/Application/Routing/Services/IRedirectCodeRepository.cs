using SmartQr.Domain.Codes.Core.Entities;

namespace SmartQr.Redirect.Api.Application.Routing.Services;

/// <summary>Defines the hot code lookup by slug — the only data dependency on the redirect hot path.</summary>
/// <remarks>The default reads the code and its rules per scan; a caching implementation may front it.</remarks>
public interface IRedirectCodeRepository
{
    /// <summary>Gets the code for a slug with its routing rules loaded, or null when the slug is unknown.</summary>
    /// <param name="slug">The printed slug the scan resolved to.</param>
    /// <param name="ct">The cancellation token.</param>
    Task<CodeEntity?> GetAsync(string slug, CancellationToken ct);
}
