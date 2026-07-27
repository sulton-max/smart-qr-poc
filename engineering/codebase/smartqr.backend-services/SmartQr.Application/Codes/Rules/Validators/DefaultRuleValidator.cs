using FluentValidation;
using SmartQr.Application.Codes.Content.Validators;
using SmartQr.Domain.Codes.Rules.Models;

namespace SmartQr.Application.Codes.Rules.Validators;

/// <summary>Validates the catch-all rule — it carries only the content it serves, since it is never matched.</summary>
/// <remarks>Delegates the payload: a member added to a <c>*Content</c> type needs its rule added there, not here.</remarks>
/// <seealso cref="CodeContentValidator"/>
public sealed class DefaultRuleValidator : AbstractValidator<DefaultRule>
{
    /// <summary>Builds the catch-all rule's rules.</summary>
    public DefaultRuleValidator() =>
        RuleFor(rule => rule.Content)
            .SetValidator(new CodeContentValidator());
}
