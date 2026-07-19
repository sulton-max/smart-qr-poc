using FluentValidation;
using FluentValidation.Results;
using SmartQr.Application.Codes.Content;
using SmartQr.Domain.Codes.Content;

namespace SmartQr.Application.Codes.Core.Validation;

/// <summary>Bridges content-type checks into FluentValidation — runs a content type's own field validation, surfacing failures in the standard ProblemDetails <c>errors</c> envelope.</summary>
internal static class ContentValidation
{
    /// <summary>Runs the resolved spec's field validation; a no-op when no content is supplied.</summary>
    public static void Apply<T>(CodeContent? content, ValidationContext<T> ctx)
    {
        if (content is null)
            return;

        // A content type with a backend spec (e.g. mobileApp) validates its own fields.
        if (ContentTypes.Resolve(CodeContent.Subtypes.KindOf(content)) is { } spec)
            foreach (var error in spec.Validate(content))
                ctx.AddFailure(new ValidationFailure(error.Property, error.Message) { ErrorCode = error.Code });
    }
}
