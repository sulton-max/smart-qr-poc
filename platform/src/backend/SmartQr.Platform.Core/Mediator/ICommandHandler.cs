using MediatR;

namespace SmartQr.Common.Mediator;

/// <summary>Defines a handler for a command that returns a result.</summary>
public interface ICommandHandler<in TCommand, TResult> : IRequestHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>;

/// <summary>Defines a handler for a command with no return value.</summary>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand>
    where TCommand : ICommand;
