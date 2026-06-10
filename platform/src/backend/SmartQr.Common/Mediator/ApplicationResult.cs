namespace SmartQr.Common.Mediator;

/// <summary>Represents the outcome of an application operation — succeeds with typed data or fails with typed error details.</summary>
public abstract record ApplicationResult<TSuccess, TFailure>
    where TSuccess : ISuccessResult
    where TFailure : IFailureResult
{
    private ApplicationResult() { }

    /// <summary>A successful operation with data and optional context.</summary>
    public sealed record Success(TSuccess Data, IApplicationSuccessContext? Context = null) : ApplicationResult<TSuccess, TFailure>;

    /// <summary>A failed operation with error and optional context.</summary>
    public sealed record Failure(TFailure Error, IApplicationFailureContext? Context = null) : ApplicationResult<TSuccess, TFailure>;
}
