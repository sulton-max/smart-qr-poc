using FluentValidation;
using SmartQr.Application.Codes.Core.Commands;
using SmartQr.Application.Codes.Rules.Models;
using SmartQr.Application.Codes.Rules.Validators;

namespace SmartQr.Application.Codes.Core.Validators;

/// <summary>Validates create-code input — the name, each rule and its content, and the rule set as a whole.</summary>
/// <remarks>A new <see cref="CodeRule"/> or <see cref="CodeRuleSet"/> member gets its rule there, not here.</remarks>
/// <seealso cref="CodeRuleValidator"/>
/// <seealso cref="CodeRuleSetValidator"/>
public sealed class CodeCreateCommandValidator : AbstractValidator<CodeCreateCommand>
{
    /// <summary>Builds the create-code rules.</summary>
    public CodeCreateCommandValidator()
    {
        RuleFor(command => command.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must be 200 characters or fewer.");

        RuleForEach(command => command.Rules).SetValidator(new CodeRuleValidator());

        RuleFor(command => new CodeRuleSet(command.Mode, command.ContentType, command.Rules))
            .SetValidator(new CodeRuleSetValidator());
    }
}
