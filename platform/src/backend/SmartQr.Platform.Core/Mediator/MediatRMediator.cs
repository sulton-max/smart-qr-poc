using MediatR;

namespace SmartQr.Common.Mediator;

/// <summary>Wraps MediatR.ISender behind <see cref="IMediator"/>. Swap this to migrate off MediatR.</summary>
internal sealed class MediatRMediator(ISender sender) : IMediator
{
    public Task<TResult> SendAsync<TResult>(IQuery<TResult> query, CancellationToken ct = default)
        => sender.Send(query, ct);

    public Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken ct = default)
        => sender.Send(command, ct);

    public Task SendAsync(ICommand command, CancellationToken ct = default)
        => sender.Send(command, ct);
}
