namespace SmartQr.Common.Mediator;

/// <summary>SmartQr's mediator abstraction — hides MediatR behind a SmartQr-owned interface.</summary>
/// <example>Injected into controllers and services to dispatch queries and commands.</example>
public interface IMediator
{
    /// <summary>Sends a query and returns the result.</summary>
    Task<TResult> SendAsync<TResult>(IQuery<TResult> query, CancellationToken ct = default);

    /// <summary>Sends a command and returns the result.</summary>
    Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken ct = default);

    /// <summary>Sends a command with no return value.</summary>
    Task SendAsync(ICommand command, CancellationToken ct = default);
}
