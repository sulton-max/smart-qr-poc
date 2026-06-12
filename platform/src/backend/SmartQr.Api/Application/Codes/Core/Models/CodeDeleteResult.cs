using SmartQr.Common.Mediator;

namespace SmartQr.Api.Application.Codes.Core.Models;

/// <summary>Outcome of deleting a code.</summary>
public abstract record CodeDeleteResult
{
    private CodeDeleteResult() { }

    /// <summary>Deleted successfully.</summary>
    public sealed record Success : CodeDeleteResult, ISuccessResult;

    /// <summary>Not found or errored. <paramref name="NotFound"/> distinguishes 404 from 500.</summary>
    public sealed record Failure(string ErrorMessage, bool NotFound = false) : CodeDeleteResult, IFailureResult;
}
