using SmartQr.Api.Application.Billing.Core.Models;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Api.Application.Billing.Core.Queries;

/// <summary>Reads the caller's billing snapshot — plan, status, limits, and live code usage.</summary>
public sealed record BillingMeQuery
    : IQuery<AppResult<BillingMeResult.Success>>
{
    /// <summary>The id of the user whose snapshot is read.</summary>
    public required Guid UserId { get; init; }
}
