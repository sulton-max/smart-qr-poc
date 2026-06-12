using SmartQr.Common.Mediator;

namespace SmartQr.Api.Application.Codes.Core.Models;

/// <summary>Outcome of enabling or disabling a code.</summary>
public abstract record CodeSetActiveResult
{
    private CodeSetActiveResult() { }

    /// <summary>Toggled successfully.</summary>
    public sealed record Success(CodeDto Code) : CodeSetActiveResult, ISuccessResult;

    /// <summary>Not found or errored. <paramref name="NotFound"/> distinguishes 404 from 500.</summary>
    public sealed record Failure(string ErrorMessage, bool NotFound = false) : CodeSetActiveResult, IFailureResult;
}
