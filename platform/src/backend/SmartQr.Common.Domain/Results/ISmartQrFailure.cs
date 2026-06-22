using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Common.Domain.Results;

/// <summary>Product-side failure shape refining the SDK's <see cref="IFailureResult"/> marker with a message and a <see cref="FailureCategory"/>.</summary>
public interface ISmartQrFailure : IFailureResult
{
    /// <summary>Gets the human-readable error message surfaced as the ProblemDetails detail.</summary>
    string ErrorMessage { get; }

    /// <summary>Gets the failure category that drives the HTTP status code.</summary>
    FailureCategory Category { get; }
}
