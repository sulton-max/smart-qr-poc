using SmartQr.Common.Domain.Results;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Api.Application.Codes.Core.Models;

/// <summary>Outcome of updating a code.</summary>
public abstract record CodeUpdateResult
{
    private CodeUpdateResult() { }

    /// <summary>Updated successfully.</summary>
    public sealed record Success(CodeDto Code) : CodeUpdateResult, ISuccessResult;

    /// <summary>Not found or errored — <see cref="ISmartQrFailure.Category"/> maps the status (NotFound → 404).</summary>
    public sealed record Failure(string ErrorMessage, FailureCategory Category) : CodeUpdateResult, ISmartQrFailure;
}
