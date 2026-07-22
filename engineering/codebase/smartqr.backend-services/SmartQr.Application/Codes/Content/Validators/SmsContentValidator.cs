using FluentValidation;
using SmartQr.Domain.Codes.Content.Sms.Models;

namespace SmartQr.Application.Codes.Content.Validators;

/// <summary>Validates a pre-filled SMS.</summary>
public sealed class SmsContentValidator : AbstractValidator<SmsContent>
{
    /// <summary>Builds the sms-content rules.</summary>
    public SmsContentValidator() =>
        RuleFor(content => content.Phone).NotEmpty().WithMessage("Phone number is required.");
}
