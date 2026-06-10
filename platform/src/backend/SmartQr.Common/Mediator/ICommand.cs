using MediatR;

namespace SmartQr.Common.Mediator;

/// <summary>Defines a command — write operation that returns a result.</summary>
/// <example>Creating a code, updating its routing rules, or rotating a slug.</example>
public interface ICommand<out TResult> : IRequest<TResult>;

/// <summary>Defines a command with no return value.</summary>
/// <example>Recording a scan, invalidating a cached config, or deleting a code.</example>
public interface ICommand : IRequest;
