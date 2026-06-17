using SmartQr.Common.Domain.Results;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Api.Application.Codes.Core.Models;

/// <summary>Outcome of fetching a single code.</summary>
public abstract record CodeGetByIdResult
{
    private CodeGetByIdResult() { }

    /// <summary>Found.</summary>
    public sealed record Success(CodeDto Code) : CodeGetByIdResult, ISuccessResult;

    /// <summary>Not found or errored — <see cref="ISmartQrFailure.Category"/> maps the status (NotFound → 404).</summary>
    public sealed record Failure(string ErrorMessage, FailureCategory Category) : CodeGetByIdResult, ISmartQrFailure;
}
