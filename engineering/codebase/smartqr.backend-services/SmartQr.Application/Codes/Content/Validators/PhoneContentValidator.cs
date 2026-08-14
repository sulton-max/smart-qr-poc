using FluentValidation;
using SmartQr.Domain.Codes.Content.Phone.Models;

namespace SmartQr.Application.Codes.Content.Validators;

/// <summary>Validates a dialed phone number.</summary>
public sealed class PhoneContentValidator : AbstractValidator<PhoneContentValueObject>
{
    /// <summary>Builds the phone-content rules.</summary>
    public PhoneContentValidator() =>
        RuleFor(content => content.Phone).NotEmpty().WithMessage("Phone number is required.");
}
