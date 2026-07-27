using FluentValidation;
using SmartQr.Application.Codes.Content.Validators;
using SmartQr.Domain.Codes.Rules.Models;

namespace SmartQr.Application.Codes.Rules.Validators;

/// <summary>Validates a conditional rule — the operand it matches a scan against, its position, and the content it serves.</summary>
/// <remarks>Delegates the payload: a member added to a <c>*Content</c> type needs its rule added there, not here.</remarks>
/// <seealso cref="CodeContentValidator"/>
public sealed class ConditionalRuleValidator : AbstractValidator<ConditionalRule>
{
    /// <summary>Builds the conditional-rule rules.</summary>
    public ConditionalRuleValidator()
    {
        RuleFor(rule => rule.Order)
            .GreaterThan(0).WithMessage("Rule order must be greater than zero.");

        RuleFor(rule => rule.ConditionValue)
            .NotEmpty().WithMessage("Rule condition value is required.");

        RuleFor(rule => rule.Content)
            .SetValidator(new CodeContentValidator());
    }
}
