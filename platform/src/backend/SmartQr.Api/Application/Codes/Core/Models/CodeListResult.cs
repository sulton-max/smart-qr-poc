using SmartQr.Common.Mediator;

namespace SmartQr.Api.Application.Codes.Core.Models;

/// <summary>Outcome of listing a owner's codes.</summary>
public abstract record CodeListResult
{
    private CodeListResult() { }

    /// <summary>Listed successfully.</summary>
    public sealed record Success(IReadOnlyList<CodeDto> Codes) : CodeListResult, ISuccessResult;

    /// <summary>Listing failed.</summary>
    public sealed record Failure(string ErrorMessage) : CodeListResult, IFailureResult;
}
