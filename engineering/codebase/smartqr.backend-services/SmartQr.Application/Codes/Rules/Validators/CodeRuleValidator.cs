using FluentValidation;
using SmartQr.Domain.Codes.Rules.Models;

namespace SmartQr.Application.Codes.Rules.Validators;

/// <summary>Validates one rule, dispatching to the validator for its role.</summary>
/// <remarks>
/// Register a new rule role here and in its own validator; FluentValidation resolves the concrete type at run time.
/// Mirrors <see cref="Content.Validators.CodeContentValidator"/> — one dispatch idiom for both unions.
/// </remarks>
public sealed class CodeRuleValidator : AbstractValidator<CodeRule>
{
    /// <summary>Builds the per-role rule dispatch.</summary>
    public CodeRuleValidator() =>
        RuleFor(rule => rule).SetInheritanceValidator(v =>
        {
            v.Add(new ConditionalRuleValidator());
            v.Add(new DefaultRuleValidator());
            v.Add(new DefaultPointerRuleValidator());
        });
}
