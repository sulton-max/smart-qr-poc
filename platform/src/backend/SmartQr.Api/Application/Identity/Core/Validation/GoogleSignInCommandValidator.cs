using FluentValidation;
using SmartQr.Api.Application.Identity.Core.Commands;

namespace SmartQr.Api.Application.Identity.Core.Validation;

/// <summary>Validates sign-in input shape — the Google ID token must be present before the verifier is called.</summary>
public sealed class GoogleSignInCommandValidator : AbstractValidator<GoogleSignInCommand>
{
    /// <summary>Builds the sign-in rules.</summary>
    public GoogleSignInCommandValidator()
    {
        RuleFor(c => c.IdToken)
            .NotEmpty().WithMessage("A Google ID token is required.");
    }
}
