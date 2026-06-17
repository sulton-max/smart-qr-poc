using SmartQr.Common.Domain.Results;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Api.Application.Codes.Core.Models;

/// <summary>Outcome of creating a code.</summary>
public abstract record CodeCreateResult
{
    private CodeCreateResult() { }

    /// <summary>Created successfully.</summary>
    public sealed record Success(CodeDto Code) : CodeCreateResult, ISuccessResult;

    /// <summary>Creation failed — <see cref="ISmartQrFailure.Category"/> maps the status (PaymentRequired → 402 when the plan cap is hit).</summary>
    public sealed record Failure(string ErrorMessage, FailureCategory Category) : CodeCreateResult, ISmartQrFailure;
}
