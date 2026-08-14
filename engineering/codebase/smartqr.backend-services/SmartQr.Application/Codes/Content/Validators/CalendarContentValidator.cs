using FluentValidation;
using SmartQr.Domain.Codes.Content.Calendar.Models;

namespace SmartQr.Application.Codes.Content.Validators;

/// <summary>Validates a calendar event.</summary>
public sealed class CalendarContentValidator : AbstractValidator<CalendarContentValueObject>
{
    /// <summary>Builds the calendar-content rules.</summary>
    public CalendarContentValidator()
    {
        RuleFor(content => content.Title).NotEmpty().WithMessage("Title is required.");

        RuleFor(content => content.End)
            .Must((content, end) => end > content.Start).WithMessage("End must be later than the start.")
            .When(content => content.End is not null);
    }
}
