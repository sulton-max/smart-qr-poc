using FluentValidation;
using SmartQr.Application.Codes.Rules.Validation;
using SmartQr.Application.Codes.Core.Commands;

namespace SmartQr.Application.Codes.Core.Validation;

/// <summary>Validates create-code input shape — name, the fallback destination, and each routing rule.</summary>
public sealed class CodeCreateCommandValidator : AbstractValidator<CodeCreateCommand>
{
    /// <summary>Builds the create-code rules.</summary>
    public CodeCreateCommandValidator()
    {
        RuleFor(c => c.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must be 200 characters or fewer.");

        // A content type with a backend spec (e.g. mobileApp) owns its own validation → content-aware messages.

        RuleForEach(c => c.Rules).SetValidator(new CodeRuleValidator());
    }
}
