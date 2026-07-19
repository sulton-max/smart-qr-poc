using FluentValidation;
using SmartQr.Domain.Codes.Rules.Models;

namespace SmartQr.Application.Codes.Rules.Validation;

/// <summary>Validates one routing rule's shape, dispatching on its role — a conditional rule needs an order and an operand, a pointer needs a target.</summary>
public sealed class CodeRuleValidator : AbstractValidator<CodeRule>
{
    /// <summary>Builds the per-role rule-shape rules.</summary>
    public CodeRuleValidator()
    {
        When(rule => rule is ConditionalRule, () =>
        {
            RuleFor(rule => ((ConditionalRule)rule).Order)
                .GreaterThan(0).WithMessage("Rule order must be greater than zero.");

            RuleFor(rule => ((ConditionalRule)rule).ConditionValue)
                .NotEmpty().WithMessage("Rule condition value is required.");
        });

        When(rule => rule is DefaultPointerRule, () =>
            RuleFor(rule => ((DefaultPointerRule)rule).TargetOrder)
                .GreaterThan(0).WithMessage("Default pointer must target a rule order greater than zero."));
    }
}
