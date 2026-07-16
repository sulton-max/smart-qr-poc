using SmartQr.Application.Codes.Core.Models;
using SmartQr.Domain.Codes.Content;
using SmartQr.Domain.Codes.Core.Enums;

namespace SmartQr.Application.Codes.Content;

/// <summary>
/// A content-type strategy: validates a content type's own fields and projects them into the persisted routing shape.
/// Lets the backend own each content model — correct, content-aware validation messages plus a single source of truth
/// for how a content type maps to a code's fallback + rules — instead of trusting the client's mapping.
/// </summary>
public interface IContentTypeSpec
{
    /// <summary>The content type this spec handles (matches <see cref="CodeContent.Type"/>).</summary>
    CodeContentType Type { get; }

    /// <summary>Validates the typed content; an empty list means valid. Messages are content-aware (e.g. "App Store link…"), not the generic fallback-URL message.</summary>
    IReadOnlyList<ContentError> Validate(CodeContent content);

    /// <summary>Projects the content into the code's persisted routing — the ordered rules, including an optional trailing <see cref="RuleConditionType.Default"/> catch-all.</summary>
    ContentProjection Project(CodeContent content);
}

/// <summary>A single content validation failure — lifted into the FluentValidation pipeline so it surfaces in the standard ProblemDetails <c>errors</c> array (property / message / code).</summary>
public sealed record ContentError(string Property, string Message, string Code);

/// <summary>The routing a content type derives — ordered rules, optionally ending in a <see cref="RuleConditionType.Default"/> catch-all (absent → a scan that matches no rule is <c>NotFound</c>).</summary>
public sealed record ContentProjection(IReadOnlyList<RuleDto> Rules);
