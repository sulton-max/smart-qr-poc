using FluentValidation;
using SmartQr.Common.Domain.Codes.Core.Enums;
using SmartQr.Domain.Codes.Content;
using SmartQr.Application.Codes.Rules.Models;
using SmartQr.Domain.Codes.Rules.Models;

namespace SmartQr.Application.Codes.Rules.Validators;

/// <summary>Validates a code's rule set as a whole — the invariants a single rule cannot see.</summary>
public sealed class CodeRuleSetValidator : AbstractValidator<CodeRuleSet>
{
    /// <summary>Builds the whole-set rules.</summary>
    public CodeRuleSetValidator()
    {
        // No rules means no content: a code with nothing to serve cannot exist.
        RuleFor(set => set.Rules)
            .NotEmpty().WithMessage("Add at least one rule — a code carries its content in its rules.");

        RuleFor(set => set.Rules)
            .Must(rules => Conditional(rules).Select(rule => rule.Order).Distinct().Count() == Conditional(rules).Count())
            .WithMessage("Rule order must be unique.");

        // Two catch-alls would make resolution ambiguous; the second could never be reached.
        RuleFor(set => set.Rules)
            .Must(rules => rules.Count(rule => rule is DefaultRule or DefaultPointerRule) <= 1)
            .WithMessage("A code carries at most one default rule.");

        // A pointer delegates to a conditional rule; anything else would leave the scan unresolved.
        RuleFor(set => set.Rules)
            .Must(rules => rules.OfType<DefaultPointerRule>().All(pointer =>
                Conditional(rules).Any(rule => rule.Order == pointer.TargetOrder)))
            .WithMessage("The default rule must point at an existing rule.");

        // A code is "a WiFi code" — every rule carries the same kind of content.
        RuleFor(set => set)
            .Must(set => Contents(set.Rules).All(content => CodeContent.Subtypes.KindOf(content) == set.ContentType))
            .WithMessage("Every rule must carry the code's content type.");

        // A static symbol bakes one payload, so it cannot hold a set that resolves differently per scan.
        RuleFor(set => set.Rules)
            .Must(rules => rules.Count == 1)
            .WithMessage("A static code carries exactly one rule — its symbol bakes a single payload.")
            .When(set => set.Mode is ContentMode.Static);
    }

    private static List<ConditionalRule> Conditional(IReadOnlyList<CodeRule> rules) => [.. rules.OfType<ConditionalRule>()];

    private static IEnumerable<CodeContent> Contents(IReadOnlyList<CodeRule> rules) =>
        rules.Select(rule => rule switch
        {
            ConditionalRule conditional => conditional.Content,
            DefaultRule fallback => fallback.Content,
            _ => null,
        })
        .OfType<CodeContent>();
}
