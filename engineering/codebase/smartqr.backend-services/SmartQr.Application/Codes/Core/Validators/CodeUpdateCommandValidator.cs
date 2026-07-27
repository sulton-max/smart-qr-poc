using FluentValidation;
using SmartQr.Application.Codes.Core.Commands;
using SmartQr.Application.Codes.Rules.Models;
using SmartQr.Application.Codes.Rules.Validators;

namespace SmartQr.Application.Codes.Core.Validators;

/// <summary>Validates update-code input — the name, each rule with the content it carries, and the rule set as a whole.</summary>
/// <remarks>
/// Near-identical to <see cref="CodeCreateCommandValidator"/> by nature, not by duplication: mode is absent from the
/// update contract (CM3), so the rule set is built with a null mode and the static-one-rule check cannot run here.
/// It becomes a transition constraint against the persisted mode once the entity is fetched — do not merge the two.
/// </remarks>
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
