namespace SmartQr.Api.Application.Codes.Core.Services;

/// <summary>Generates short, URL-safe, unguessable slugs for new codes.</summary>
public interface ISlugGenerator
{
    /// <summary>Returns a new random slug (uniqueness is checked by the caller against the store).</summary>
    string Next();
}
