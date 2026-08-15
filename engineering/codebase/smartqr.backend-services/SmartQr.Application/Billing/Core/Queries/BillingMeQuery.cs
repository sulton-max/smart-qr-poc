using SmartQr.Application.Billing.Core.Models;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Application.Billing.Core.Queries;

/// <summary>Represents a query to read the caller's billing snapshot.</summary>
public sealed record BillingMeQuery
    : IQuery<AppResult<BillingMeResult.Success>>
{
    /// <summary>Gets the id of the user whose snapshot is read.</summary>
    public required Guid UserId { get; init; }
}
