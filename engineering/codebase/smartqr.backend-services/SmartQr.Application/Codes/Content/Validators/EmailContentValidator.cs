using FluentValidation;
using SmartQr.Domain.Codes.Content.Email.Models;

namespace SmartQr.Application.Codes.Content.Validators;

/// <summary>Validates a pre-filled email.</summary>
public sealed class EmailContentValidator : AbstractValidator<EmailContent>
{
    /// <summary>Builds the email-content rules.</summary>
    public EmailContentValidator() =>
        RuleFor(content => content.To)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Recipient is required.")
            .EmailAddress().WithMessage("Recipient must be a valid email address.");
}
