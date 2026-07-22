using SmartQr.Application.Codes.Core.Models;
using WoW.Two.Sdk.Backend.Beta.Codes.Models.Style;
using SmartQr.Domain.Codes.Core.Entities;

namespace SmartQr.Infrastructure.Codes.Core.Extensions;

/// <summary>Maps code entities to API DTOs.</summary>
public static class CodeMappingExtensions
{
    /// <summary>Projects a <see cref="CodeEntity"/> to a <see cref="CodeDto"/>, building the short URL from the redirect base. A static code has neither slug nor short URL.</summary>
    public static CodeDto ToDto(this CodeEntity e, string redirectBaseUrl) => new()
    {
        Id = e.Id,
        Slug = e.Slug,
        ShortUrl = e.Slug is { } slug ? $"{redirectBaseUrl.TrimEnd('/')}/{slug}" : null,
        Name = e.Name,
        BarcodeFormat = e.BarcodeFormat,
        Mode = e.Mode,
        ContentType = e.ContentType,
        IsActive = e.IsActive,
        ScanCount = e.ScanCount,
        CreatedAt = e.CreatedAt,
        Rules = [.. e.Rules],
        Style = StyleSpecJson.Deserialize(e.StyleJson),
    };
}
