using FluentValidation;
using SmartQr.Domain.Codes.Content.VCard.Models;

namespace SmartQr.Application.Codes.Content.Validators;

/// <summary>Validates a contact card.</summary>
public sealed class VCardContentValidator : AbstractValidator<VCardContent>
{
    /// <summary>Builds the vCard-content rules.</summary>
    public VCardContentValidator()
    {
        RuleFor(content => content.FirstName).NotEmpty().WithMessage("First name is required.");

        // A card with no way to reach the person is a card worth nothing.
        RuleFor(content => content)
            .Must(content => !string.IsNullOrWhiteSpace(content.Phone)
                || !string.IsNullOrWhiteSpace(content.Email)
                || !string.IsNullOrWhiteSpace(content.Url))
            .WithMessage("Add at least one contact method — phone, email, or website.");
    }
}
