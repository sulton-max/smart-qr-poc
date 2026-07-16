using SmartQr.Domain.Codes.Core.Entities;

namespace SmartQr.Redirect.Api.Application.Routing.Services;

/// <summary>The hot code lookup by slug. The only data dependency on the redirect hot path.</summary>
/// <remarks>The default reads the code (with its rules) per scan; a caching implementation may front it.</remarks>
public interface IRedirectCodeRepository
{
    /// <summary>Returns the code for a slug with its routing rules loaded, or null if unknown.</summary>
    Task<CodeEntity?> GetAsync(string slug, CancellationToken ct);
}
