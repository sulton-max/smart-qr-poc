using FluentValidation;
using SmartQr.Application.Codes.Rules.Validation;
using SmartQr.Application.Codes.Core.Commands;

namespace SmartQr.Application.Codes.Core.Validation;

/// <summary>Validates update-code input shape — same field rules as create (the slug is preserved, not re-supplied).</summary>
public sealed class CodeUpdateCommandValidator : AbstractValidator<CodeUpdateCommand>
{
    /// <summary>Builds the update-code rules.</summary>
    public CodeUpdateCommandValidator()
    {
        RuleFor(c => c.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must be 200 characters or fewer.");

        // A content type with a backend spec (e.g. mobileApp) owns its own validation → content-aware messages.

        RuleForEach(c => c.Rules).SetValidator(new CodeRuleValidator());
    }
}
