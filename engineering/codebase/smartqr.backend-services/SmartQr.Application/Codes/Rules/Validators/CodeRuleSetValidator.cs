using FluentValidation;
using SmartQr.Common.Domain.Codes.Core.Enums;
using SmartQr.Domain.Codes.Content;
using SmartQr.Application.Codes.Rules.Models;
using SmartQr.Domain.Codes.Rules.Models;

namespace SmartQr.Application.Codes.Rules.Validators;

/// <summary>Validates a code's rule set as a whole.</summary>
/// <remarks>Give each rule an explicit <c>OverridePropertyName</c>; <c>CodeValidationPathTests</c> locks it.</remarks>
public sealed class CodeRuleSetValidator : AbstractValidator<CodeRuleSet>
{
    /// <summary>Builds the whole-set rules.</summary>
    public CodeRuleSetValidator()
    {
        // No rules means no content: a code with nothing to serve cannot exist.
        RuleFor(set => set.Rules)
            .NotEmpty().WithMessage("Add at least one rule — a code carries its content in its rules.")
            .OverridePropertyName(nameof(CodeRuleSet.Rules));

        RuleFor(set => set.Rules)
            .Must(rules => Conditional(rules).Select(rule => rule.Order).Distinct().Count()
                           == Conditional(rules).Count())
            .WithMessage("Rule order must be unique.")
            .OverridePropertyName(nameof(CodeRuleSet.Rules));

        // Two catch-alls would make resolution ambiguous; the second could never be reached.
        RuleFor(set => set.Rules)
            .Must(rules => rules.Count(rule => rule is DefaultRuleValueObject or DefaultPointerRuleValueObject) <= 1)
            .WithMessage("A code carries at most one default rule.")
            .OverridePropertyName(nameof(CodeRuleSet.Rules));

        // A pointer delegates to a conditional rule; anything else would leave the scan unresolved.
        RuleFor(set => set.Rules)
            .Must(rules => rules.OfType<DefaultPointerRuleValueObject>().All(pointer =>
                Conditional(rules).Any(rule => rule.Order == pointer.TargetOrder)))
            .WithMessage("The default rule must point at an existing rule.")
            .OverridePropertyName(nameof(CodeRuleSet.Rules));

        // A code is "a WiFi code" — every rule carries the same kind of content. Names ContentType: that is the
        // member the caller picked, and the rules were seeded from it.
        RuleFor(set => set)
            .Must(set => Contents(set.Rules)
                .All(content => CodeContentValueObject.Subtypes.KindOf(content) == set.ContentType))
            .WithMessage("Every rule must carry the code's content type.")
            .OverridePropertyName(nameof(CodeRuleSet.ContentType));

        // A static symbol bakes one payload, so it cannot hold a set that resolves differently per scan.
        // Names Mode: switching to dynamic is the fix the caller reaches for, and removing rules is the other.
        RuleFor(set => set.Rules)
            .Must(rules => rules.Count == 1)
            .WithMessage("A static code carries exactly one rule — its symbol bakes a single payload.")
            .When(set => set.Mode is ContentMode.Static)
            .OverridePropertyName(nameof(CodeRuleSet.Mode));
    }

    private static List<ConditionalRuleValueObject> Conditional(IReadOnlyList<CodeRuleValueObject> rules) =>
        [.. rules.OfType<ConditionalRuleValueObject>()];

    private static IEnumerable<CodeContentValueObject> Contents(IReadOnlyList<CodeRuleValueObject> rules) =>
        rules.Select(rule => rule switch
        {
            ConditionalRuleValueObject conditional => conditional.Content,
            DefaultRuleValueObject fallback => fallback.Content,
            _ => null,
        })
        .OfType<CodeContentValueObject>();
}
