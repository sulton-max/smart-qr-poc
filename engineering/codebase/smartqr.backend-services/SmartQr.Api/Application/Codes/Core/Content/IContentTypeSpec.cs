using SmartQr.Api.Application.Codes.Core.Models;

namespace SmartQr.Api.Application.Codes.Core.Content;

/// <summary>
/// A content-type strategy: validates a content type's own fields and projects them into the persisted routing shape.
/// Lets the backend own each content model — correct, content-aware validation messages plus a single source of truth
/// for how a content type maps to a code's fallback + rules — instead of trusting the client's mapping.
/// </summary>
public interface IContentTypeSpec
{
    /// <summary>The content-type id this spec handles (matches <see cref="ContentSpec.Type"/>, e.g. <c>mobileApp</c>).</summary>
    string Type { get; }

    /// <summary>Validates the content's fields; an empty list means valid. Messages are content-aware (e.g. "App Store link…"), not the generic fallback-URL message.</summary>
    IReadOnlyList<ContentError> Validate(ContentSpec content);

    /// <summary>Projects the content into the code's persisted routing — the derived fallback destination plus ordered device rules.</summary>
    ContentProjection Project(ContentSpec content);
}

/// <summary>A single content validation failure — lifted into the FluentValidation pipeline so it surfaces in the standard ProblemDetails <c>errors</c> array (property / message / code).</summary>
public sealed record ContentError(string Property, string Message, string Code);

/// <summary>The routing a content type derives — the fallback destination plus ordered rules (empty for a single-destination code).</summary>
public sealed record ContentProjection(string FallbackUrl, IReadOnlyList<RuleDto> Rules);
