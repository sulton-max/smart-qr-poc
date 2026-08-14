using FluentValidation;
using SmartQr.Application.Codes.Validators;
using SmartQr.Domain.Codes.Content.MobileApp.Models;

namespace SmartQr.Application.Codes.Content.Validators;

/// <summary>Validates one app-store link.</summary>
public sealed class MobileAppLinkContentValidator : AbstractValidator<MobileAppLinkContentValueObject>
{
    /// <summary>Builds the store-link rules.</summary>
    public MobileAppLinkContentValidator() =>
        RuleFor(content => content.Url)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Store URL is required.")
            .Must(CodeValidationRules.IsAbsoluteHttpUrl).WithMessage("Store URL must be an absolute http(s) URL.");
}
