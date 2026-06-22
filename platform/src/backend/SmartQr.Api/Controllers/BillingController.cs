using Microsoft.AspNetCore.Mvc;
using SmartQr.Api.Application.Billing.Core.Commands;
using SmartQr.Api.Application.Billing.Core.Models;
using SmartQr.Api.Application.Billing.Core.Queries;
using SmartQr.Api.Application.Identity.Core.Services;
using SmartQr.Api.Requests;
using WoW.Two.Sdk.Backend.Beta.Mediator;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;
using WoW.Two.Sdk.Backend.Beta.Web.Contracts;

namespace SmartQr.Api.Controllers;

/// <summary>Manages billing.</summary>
[ApiController]
[Route("api/billing")]
public sealed class BillingController(ISender sender, ICurrentUser currentUser) : ControllerBase
{
    /// <summary>Starts a checkout session.</summary>
    [HttpPost("checkout")]
    [ProducesResponseType<ApiResponse<CheckoutSessionDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Checkout([FromBody] CheckoutBillingApiRequest request, CancellationToken ct)
    {
        if (currentUser.Id is not { } userId)
            return Unauthorized();

        var command = request.ToCommand(userId);
        var result = await sender.SendAsync(command, ct);

        return result.Match<IActionResult>(
            ok => Ok(ApiResponse<CheckoutSessionDto>.Ok(ok.Data.Session)),
            fail => Problem(detail: fail.Error.ErrorMessage, statusCode: ApiResults.ToStatusCode(fail.Error.Category)));
    }

    /// <summary>Opens a billing portal session.</summary>
    [HttpPost("portal")]
    [ProducesResponseType<ApiResponse<PortalSessionDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Portal(CancellationToken ct)
    {
        if (currentUser.Id is not { } userId)
            return Unauthorized();

        var command = new BillingPortalCommand { UserId = userId };
        var result = await sender.SendAsync(command, ct);

        return result.Match<IActionResult>(
            ok => Ok(ApiResponse<PortalSessionDto>.Ok(ok.Data.Session)),
            fail => Problem(detail: fail.Error.ErrorMessage, statusCode: ApiResults.ToStatusCode(fail.Error.Category)));
    }

    /// <summary>Handles a Stripe webhook event.</summary>
    /// <remarks>Not owner-scoped or enveloped; verifies the raw body and signature, returning 400 on a bad signature so Stripe retries.</remarks>
    [HttpPost("webhook")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Webhook(CancellationToken ct)
    {
        // Read the body verbatim — signature verification hashes the exact bytes, so no model binding.
        using var reader = new StreamReader(Request.Body);
        var rawBody = await reader.ReadToEndAsync(ct);
        var signature = Request.Headers["Stripe-Signature"].ToString();

        var result = await sender.SendAsync(
            new BillingWebhookCommand { RawBody = rawBody, StripeSignature = signature }, ct);

        return result.Match<IActionResult>(
            _ => Ok(),
            fail => Problem(detail: fail.Error.ErrorMessage, statusCode: ApiResults.ToStatusCode(fail.Error.Category)));
    }

    /// <summary>Gets the current billing status.</summary>
    [HttpGet("me")]
    [ProducesResponseType<ApiResponse<BillingStatusDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        if (currentUser.Id is not { } userId)
            return Unauthorized();

        var query = new BillingMeQuery { UserId = userId };
        var result = await sender.SendAsync(query, ct);

        return result.Match<IActionResult>(
            ok => Ok(ApiResponse<BillingStatusDto>.Ok(ok.Data.Status)),
            fail => Problem(detail: fail.Error.ErrorMessage, statusCode: ApiResults.ToStatusCode(fail.Error.Category)));
    }
}
