using FluentValidation;
using SmartQr.Application.Identity.Core.Commands;

namespace SmartQr.Application.Identity.Core.Validators;

/// <summary>Validates sign-in input.</summary>
public sealed class GoogleSignInCommandValidator : AbstractValidator<GoogleSignInCommand>
{
    /// <summary>Builds the sign-in rules.</summary>
    public GoogleSignInCommandValidator()
    {
        RuleFor(c => c.IdToken)
            .NotEmpty().WithMessage("A Google ID token is required.");
    }
}
