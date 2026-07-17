using SmartQr.Application.Codes.Core.Models;
using WoW.Two.Sdk.Backend.Beta.Codes.Models.Style;
using SmartQr.Domain.Codes.Core.Entities;

namespace SmartQr.Infrastructure.Codes.Core.Extensions;

/// <summary>Maps code entities to API DTOs.</summary>
public static class CodeMappingExtensions
{
    /// <summary>Projects a <see cref="CodeEntity"/> to a <see cref="CodeDto"/>, building the short URL from the redirect base.</summary>
    public static CodeDto ToDto(this CodeEntity e, string redirectBaseUrl) => new()
    {
        Id = e.Id,
        Slug = e.Slug,
        ShortUrl = $"{redirectBaseUrl.TrimEnd('/')}/{e.Slug}",
        Name = e.Name,
        BarcodeFormat = e.BarcodeFormat,
        IsActive = e.IsActive,
        ScanCount = e.ScanCount,
        CreatedAt = e.CreatedAt,
        Rules = e.Rules
            .OrderBy(r => r.Order)
            .Select(r => new RuleDto
            {
                Order = r.Order,
                ConditionType = r.ConditionType,
                ConditionValue = r.ConditionValue,
                Destination = r.Destination,
            })
            .ToList(),
        Style = StyleSpecJson.Deserialize(e.StyleJson),
        Content = e.Content,
    };
}
