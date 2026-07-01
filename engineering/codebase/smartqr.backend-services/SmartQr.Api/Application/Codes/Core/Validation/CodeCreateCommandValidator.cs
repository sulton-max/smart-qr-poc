using FluentValidation;
using SmartQr.Api.Application.Codes.Core.Commands;
using SmartQr.Api.Application.Codes.Core.Content;

namespace SmartQr.Api.Application.Codes.Core.Validation;

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
