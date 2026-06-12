using SmartQr.Common.Mediator;

namespace SmartQr.Api.Application.Codes.Core.Models;

/// <summary>Outcome of updating a code.</summary>
public abstract record CodeUpdateResult
{
    private CodeUpdateResult() { }

    /// <summary>Updated successfully.</summary>
    public sealed record Success(CodeDto Code) : CodeUpdateResult, ISuccessResult;

    /// <summary>Not found or errored. <paramref name="NotFound"/> distinguishes 404 from 500.</summary>
    public sealed record Failure(string ErrorMessage, bool NotFound = false) : CodeUpdateResult, IFailureResult;
}
