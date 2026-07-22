using FluentValidation;
using SmartQr.Domain.Codes.Content.Geo.Models;

namespace SmartQr.Application.Codes.Content.Validators;

/// <summary>Validates a geographic location.</summary>
public sealed class GeoContentValidator : AbstractValidator<GeoContent>
{
    /// <summary>Builds the geo-content rules.</summary>
    public GeoContentValidator()
    {
        RuleFor(content => content.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90.");

        RuleFor(content => content.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180.");
    }
}
