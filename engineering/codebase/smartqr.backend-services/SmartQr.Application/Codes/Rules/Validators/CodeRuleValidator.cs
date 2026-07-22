using FluentValidation;
using SmartQr.Application.Codes.Content.Validators;
using SmartQr.Domain.Codes.Rules.Models;

namespace SmartQr.Application.Codes.Rules.Validators;

/// <summary>Validates one rule, dispatching on its role — a conditional rule needs an order and an operand, a pointer a target, and either content-bearing role its content.</summary>
public sealed class CodeRuleValidator : AbstractValidator<CodeRule>
{
    /// <summary>Builds the per-role rule rules.</summary>
    public CodeRuleValidator()
    {
        When(rule => rule is ConditionalRule, () =>
        {
            RuleFor(rule => ((ConditionalRule)rule).Order)
                .GreaterThan(0).WithMessage("Rule order must be greater than zero.");

            RuleFor(rule => ((ConditionalRule)rule).ConditionValue)
                .NotEmpty().WithMessage("Rule condition value is required.");

            RuleFor(rule => ((ConditionalRule)rule).Content)
                .SetValidator(new CodeContentValidator());
        });

        When(rule => rule is DefaultRule, () =>
            RuleFor(rule => ((DefaultRule)rule).Content)
                .SetValidator(new CodeContentValidator()));

        When(rule => rule is DefaultPointerRule, () =>
            RuleFor(rule => ((DefaultPointerRule)rule).TargetOrder)
                .GreaterThan(0).WithMessage("Default pointer must target a rule order greater than zero."));
    }
}
