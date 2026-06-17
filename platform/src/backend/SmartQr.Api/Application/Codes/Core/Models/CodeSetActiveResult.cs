using SmartQr.Common.Domain.Results;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Api.Application.Codes.Core.Models;

/// <summary>Outcome of enabling or disabling a code.</summary>
public abstract record CodeSetActiveResult
{
    private CodeSetActiveResult() { }

    /// <summary>Toggled successfully.</summary>
    public sealed record Success(CodeDto Code) : CodeSetActiveResult, ISuccessResult;

    /// <summary>Not found or errored — <see cref="ISmartQrFailure.Category"/> maps the status (NotFound → 404).</summary>
    public sealed record Failure(string ErrorMessage, FailureCategory Category) : CodeSetActiveResult, ISmartQrFailure;
}
