using FluentValidation;
using SmartQr.Api.Application.Codes.Core.Commands;

namespace SmartQr.Api.Application.Codes.Core.Validation;

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

        RuleFor(c => c.FallbackUrl)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("A fallback URL is required.")
            .Must(CodeValidationRules.IsAbsoluteHttpUrl).WithMessage("The fallback URL must be an absolute http(s) URL.");

        RuleForEach(c => c.Rules).SetValidator(new RuleDtoValidator());
    }
}
