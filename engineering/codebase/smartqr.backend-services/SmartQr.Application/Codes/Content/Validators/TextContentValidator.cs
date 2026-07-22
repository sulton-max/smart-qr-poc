using FluentValidation;
using SmartQr.Domain.Codes.Content.Text.Models;

namespace SmartQr.Application.Codes.Content.Validators;

/// <summary>Validates free-text content.</summary>
public sealed class TextContentValidator : AbstractValidator<TextContent>
{
    /// <summary>Builds the text-content rules.</summary>
    public TextContentValidator() =>
        RuleFor(content => content.Text).NotEmpty().WithMessage("Text is required.");
}
