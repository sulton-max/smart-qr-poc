using SmartQr.Common.Mediator;

namespace SmartQr.Api.Application.Codes.Core.Models;

/// <summary>Outcome of fetching a single code.</summary>
public abstract record CodeGetByIdResult
{
    private CodeGetByIdResult() { }

    /// <summary>Found.</summary>
    public sealed record Success(CodeDto Code) : CodeGetByIdResult, ISuccessResult;

    /// <summary>Not found or errored. <paramref name="NotFound"/> distinguishes 404 from 500.</summary>
    public sealed record Failure(string ErrorMessage, bool NotFound = false) : CodeGetByIdResult, IFailureResult;
}
