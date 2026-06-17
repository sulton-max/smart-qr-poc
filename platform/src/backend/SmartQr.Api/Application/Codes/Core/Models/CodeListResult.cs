using SmartQr.Common.Domain.Results;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Api.Application.Codes.Core.Models;

/// <summary>Outcome of listing a owner's codes.</summary>
public abstract record CodeListResult
{
    private CodeListResult() { }

    /// <summary>Listed successfully.</summary>
    public sealed record Success(IReadOnlyList<CodeDto> Codes) : CodeListResult, ISuccessResult;

    /// <summary>Listing failed — <see cref="ISmartQrFailure.Category"/> maps the status.</summary>
    public sealed record Failure(string ErrorMessage, FailureCategory Category) : CodeListResult, ISmartQrFailure;
}
