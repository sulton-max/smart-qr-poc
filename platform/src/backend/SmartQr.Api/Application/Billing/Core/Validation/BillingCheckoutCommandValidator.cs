using FluentValidation;
using SmartQr.Api.Application.Billing.Core.Commands;

namespace SmartQr.Api.Application.Billing.Core.Validation;

/// <summary>Validates checkout input shape — the requested plan must be a defined enum value.</summary>
public sealed class BillingCheckoutCommandValidator : AbstractValidator<BillingCheckoutCommand>
{
    /// <summary>Builds the checkout rules.</summary>
    public BillingCheckoutCommandValidator()
    {
        RuleFor(c => c.Plan)
            .IsInEnum().WithMessage("Plan must be a known plan.");
    }
}
