using FluentValidation;
using SmartQr.Application.Codes.Core.Models;

namespace SmartQr.Application.Codes.Core.Validation;

/// <summary>Validates a single routing rule's shape — a non-negative order and a servable destination.</summary>
public sealed class RuleDtoValidator : AbstractValidator<RuleDto>
{
    /// <summary>Builds the rule-shape rules.</summary>
    public RuleDtoValidator()
    {
        RuleFor(r => r.Order)
            .GreaterThanOrEqualTo(0).WithMessage("Rule order must be zero or greater.");

        RuleFor(r => r.Destination)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Rule destination is required.")
            .Must(CodeValidationRules.IsAbsoluteHttpUrl).WithMessage("Rule destination must be an absolute http(s) URL.");
    }
}
