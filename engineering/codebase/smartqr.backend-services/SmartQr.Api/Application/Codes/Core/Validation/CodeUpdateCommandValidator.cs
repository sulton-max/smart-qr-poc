using FluentValidation;
using SmartQr.Api.Application.Codes.Core.Commands;
using SmartQr.Api.Application.Codes.Core.Content;

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

        // A content type with a backend spec (e.g. mobileApp) owns its own validation → content-aware messages.
        RuleFor(c => c).Custom((command, ctx) => ContentValidation.Apply(command.Content, ctx));

        // Generic fallback rule — only for codes with no content spec and no baked static payload (url / legacy).
        When(c => ContentTypes.Resolve(c.Content?.Type) is null && c.Content?.IsStatic != true, () =>
        {
            RuleFor(c => c.FallbackUrl)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("A fallback URL is required.")
                .Must(CodeValidationRules.IsAbsoluteHttpUrl).WithMessage("The fallback URL must be an absolute http(s) URL.");
        });

        RuleForEach(c => c.Rules).SetValidator(new RuleDtoValidator());
    }
}
