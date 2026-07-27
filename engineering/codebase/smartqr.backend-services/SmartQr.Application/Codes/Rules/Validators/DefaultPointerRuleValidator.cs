using FluentValidation;
using SmartQr.Domain.Codes.Rules.Models;

namespace SmartQr.Application.Codes.Rules.Validators;

/// <summary>Validates the pointer catch-all — it carries only the order of the rule whose content it reuses.</summary>
/// <remarks>That the target exists is a whole-set question, so it lives in <see cref="CodeRuleSetValidator"/>, not here.</remarks>
public sealed class DefaultPointerRuleValidator : AbstractValidator<DefaultPointerRule>
{
    /// <summary>Builds the pointer rule's rules.</summary>
    public DefaultPointerRuleValidator() =>
        RuleFor(rule => rule.TargetOrder)
            .GreaterThan(0).WithMessage("Default pointer must target a rule order greater than zero.");
}
