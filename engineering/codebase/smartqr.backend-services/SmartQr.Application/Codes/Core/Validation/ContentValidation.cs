using FluentValidation;
using FluentValidation.Results;
using SmartQr.Application.Codes.Core.Content;
using SmartQr.Application.Codes.Core.Models;
using SmartQr.Domain.Codes.Enums;
using SmartQr.Domain.Codes.Extensions;

namespace SmartQr.Application.Codes.Core.Validation;

/// <summary>Bridges content-type checks into FluentValidation — rejects unsupported content types and runs a content type's own field validation, surfacing failures in the standard ProblemDetails <c>errors</c> envelope.</summary>
internal static class ContentValidation
{
    /// <summary>Rejects a content type the builder can't create yet, then runs the resolved spec's field validation; a no-op when no content is supplied.</summary>
    public static void Apply<T>(ContentSpec? content, ValidationContext<T> ctx)
    {
        if (content is null)
            return;

        // Reject content types the builder can't create yet — unknown, or known-but-unsupported (e.g. youtube).
        if (!Enum.TryParse<CodeContentType>(content.Type, ignoreCase: true, out var contentType) || !contentType.IsSupported())
        {
            ctx.AddFailure(new ValidationFailure("content.type", $"The '{content.Type}' content type isn't supported yet.") { ErrorCode = "UnsupportedContentType" });
            return;
        }

        // A content type with a backend spec (e.g. mobileApp) validates its own fields.
        if (ContentTypes.Resolve(content.Type) is { } spec)
            foreach (var error in spec.Validate(content))
                ctx.AddFailure(new ValidationFailure(error.Property, error.Message) { ErrorCode = error.Code });
    }
}
