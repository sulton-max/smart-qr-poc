using FluentValidation;
using FluentValidation.Results;
using SmartQr.Api.Application.Codes.Core.Content;
using SmartQr.Api.Application.Codes.Core.Models;

namespace SmartQr.Api.Application.Codes.Core.Validation;

/// <summary>Bridges the content-type registry into FluentValidation — a content type with a backend spec validates its own fields, surfacing content-aware failures in the standard ProblemDetails <c>errors</c> envelope.</summary>
internal static class ContentValidation
{
    /// <summary>Runs the resolved content spec's validation and lifts each failure into the pipeline; a no-op for types without a backend spec.</summary>
    public static void Apply<T>(ContentSpec? content, ValidationContext<T> ctx)
    {
        if (content is null || ContentTypes.Resolve(content.Type) is not { } spec)
            return;

        foreach (var error in spec.Validate(content))
            ctx.AddFailure(new ValidationFailure(error.Property, error.Message) { ErrorCode = error.Code });
    }
}
