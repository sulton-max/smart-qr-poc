using FluentValidation;
using SmartQr.Application.Codes.Core.Commands;
using SmartQr.Application.Codes.Rules.Models;
using SmartQr.Application.Codes.Rules.Validators;

namespace SmartQr.Application.Codes.Core.Validators;

/// <summary>Validates update-code input — the name, each rule and its content, and the rule set as a whole.</summary>
/// <remarks>Do not merge with <see cref="CodeCreateCommandValidator"/>: update carries no mode (CM3).</remarks>
/// <seealso cref="CodeRuleValidator"/>
/// <seealso cref="CodeRuleSetValidator"/>
public sealed class CodeUpdateCommandValidator : AbstractValidator<CodeUpdateCommand>
{
    /// <summary>Builds the update-code rules.</summary>
    public CodeUpdateCommandValidator()
    {
        RuleFor(command => command.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must be 200 characters or fewer.");

        RuleForEach(command => command.Rules).SetValidator(new CodeRuleValidator());

        RuleFor(command => new CodeRuleSet(null, command.ContentType, command.Rules))
            .SetValidator(new CodeRuleSetValidator());
    }
}
