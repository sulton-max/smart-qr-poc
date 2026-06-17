using SmartQr.Common.Domain.Results;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Api.Application.Codes.Core.Models;

/// <summary>Outcome of deleting a code.</summary>
public abstract record CodeDeleteResult
{
    private CodeDeleteResult() { }

    /// <summary>Deleted successfully.</summary>
    public sealed record Success : CodeDeleteResult, ISuccessResult;

    /// <summary>Not found or errored — <see cref="ISmartQrFailure.Category"/> maps the status (NotFound → 404).</summary>
    public sealed record Failure(string ErrorMessage, FailureCategory Category) : CodeDeleteResult, ISmartQrFailure;
}
