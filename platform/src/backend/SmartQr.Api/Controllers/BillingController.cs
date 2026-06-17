using Microsoft.AspNetCore.Mvc;
using SmartQr.Api.Application.Billing.Core.Commands;
using SmartQr.Api.Application.Billing.Core.Models;
using SmartQr.Api.Application.Billing.Core.Queries;
using SmartQr.Api.Application.Identity.Core.Services;
using SmartQr.Api.Requests;
using SmartQr.Common.Models;
using WoW.Two.Sdk.Backend.Beta.Mediator;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Api.Controllers;

/// <summary>Manages billing.</summary>
[ApiController]
[Route("api/billing")]
public sealed class BillingController(ISender sender, ICurrentUser currentUser) : ControllerBase
{
    /// <summary>Starts a hosted Checkout session for a paid plan and returns the URL to redirect the browser to.</summary>
    [HttpPost("checkout")]
    [ProducesResponseType<ApiResponse<CheckoutSessionDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request, CancellationToken ct)
    {
        if (currentUser.Id is not { } userId)
            return Unauthorized();

        var result = await sender.SendAsync(new BillingCheckoutCommand { UserId = userId, Plan = request.Plan }, ct);

        return result.Match<BillingCheckoutResult.Success, BillingCheckoutResult.Failure, IActionResult>(
            ok => Ok(ApiResponse<CheckoutSessionDto>.Ok(ok.Data.Session)),
            fail => Problem(detail: fail.Error.ErrorMessage, statusCode: ApiResults.ToStatusCode(fail.Error.Category)));
    }

    /// <summary>Opens a Customer Portal session for the caller's stored Stripe customer.</summary>
    [HttpPost("portal")]
    [ProducesResponseType<ApiResponse<PortalSessionDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Portal(CancellationToken ct)
    {
        if (currentUser.Id is not { } userId)
            return Unauthorized();

        var result = await sender.SendAsync(new BillingPortalCommand { UserId = userId }, ct);

        return result.Match<BillingPortalResult.Success, BillingPortalResult.Failure, IActionResult>(
            ok => Ok(ApiResponse<PortalSessionDto>.Ok(ok.Data.Session)),
            fail => Problem(detail: fail.Error.ErrorMessage, statusCode: ApiResults.ToStatusCode(fail.Error.Category)));
    }

    /// <summary>
    /// Handles a Stripe webhook event — NOT owner-scoped and NOT enveloped. Reads the raw body + <c>Stripe-Signature</c>
    /// header, verifies it, and upserts the affected subscription. Returns 200 when handled/ignored, 400 on a bad
    /// signature (so Stripe's retry logic is correct).
    /// </summary>
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

        return result.Match<BillingWebhookResult.Success, BillingWebhookResult.Failure, IActionResult>(
            _ => Ok(),
            fail => Problem(detail: fail.Error.ErrorMessage, statusCode: ApiResults.ToStatusCode(fail.Error.Category)));
    }

    /// <summary>Gets the caller's plan, status, limits, and live usage. No subscription row ⇒ a Free/active default.</summary>
    [HttpGet("me")]
    [ProducesResponseType<ApiResponse<BillingStatusDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        if (currentUser.Id is not { } userId)
            return Unauthorized();

        var result = await sender.SendAsync(new BillingMeQuery { UserId = userId }, ct);

        return result.Match<BillingMeResult.Success, BillingMeResult.Failure, IActionResult>(
            ok => Ok(ApiResponse<BillingStatusDto>.Ok(ok.Data.Status)),
            fail => Problem(detail: fail.Error.ErrorMessage, statusCode: ApiResults.ToStatusCode(fail.Error.Category)));
    }
}
