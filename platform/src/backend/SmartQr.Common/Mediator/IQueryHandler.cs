using MediatR;

namespace SmartQr.Common.Mediator;

/// <summary>Defines a handler for a query that returns a result.</summary>
public interface IQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, TResult> where TQuery : IQuery<TResult>;
