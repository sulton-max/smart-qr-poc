using SmartQr.Application.Codes.Core.Models;
using WoW.Two.Sdk.Backend.Beta.Codes.Models.Style;
using SmartQr.Domain.Codes.Core.Entities;

namespace SmartQr.Infrastructure.Codes.Core.Extensions;

/// <summary>Extends <see cref="CodeEntity"/> for API DTO projection.</summary>
public static class CodeMappingExtensions
{
    /// <summary>Projects the code entity to its API DTO.</summary>
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
