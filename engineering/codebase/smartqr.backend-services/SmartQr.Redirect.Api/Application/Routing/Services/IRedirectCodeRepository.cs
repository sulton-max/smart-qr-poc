using SmartQr.Domain.Codes.Core.Entities;

namespace SmartQr.Redirect.Api.Application.Routing.Services;

/// <summary>Defines the contract for looking up a code by slug.</summary>
public interface IRedirectCodeRepository
{
    /// <summary>Gets the code for a slug with its routing rules loaded, or null when the slug is unknown.</summary>
    /// <param name="slug">The printed slug the scan resolved to.</param>
    Task<CodeEntity?> GetAsync(string slug, CancellationToken ct);
}
