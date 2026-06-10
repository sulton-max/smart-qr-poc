using MediatR;

namespace SmartQr.Common.Mediator;

/// <summary>Defines a query — read-only operation that returns a result.</summary>
/// <example>Fetching a code by id, listing a user's codes, or reading scan stats.</example>
public interface IQuery<out TResult> : IRequest<TResult>;
