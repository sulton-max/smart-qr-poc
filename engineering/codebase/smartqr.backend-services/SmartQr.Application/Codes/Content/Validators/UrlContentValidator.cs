using FluentValidation;
using SmartQr.Application.Codes.Validators;
using SmartQr.Domain.Codes.Content.Url.Models;

namespace SmartQr.Application.Codes.Content.Validators;

/// <summary>Validates url content — the destination the redirect sends a scanner to.</summary>
public sealed class UrlContentValidator : AbstractValidator<UrlContentValueObject>
{
    /// <summary>Builds the url-content rules.</summary>
    public UrlContentValidator() =>
        RuleFor(content => content.Url)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("URL is required.")
            .Must(CodeValidationRules.IsAbsoluteHttpUrl).WithMessage("URL must be an absolute http(s) URL.");
}
