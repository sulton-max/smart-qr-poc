namespace SmartQr.Redirect.Api.Application.Routing.Models;

/// <summary>The kind of response the redirect endpoint should produce.</summary>
public enum RouteOutcome
{
    /// <summary>Redirect to <see cref="RouteDecision.DestinationUrl"/> (302).</summary>
    Redirect,

    /// <summary>No such code / inactive (404).</summary>
    NotFound,

    /// <summary>Expired or scan-capped (410 Gone).</summary>
    Gone,

    /// <summary>Password gate required (interstitial).</summary>
    PasswordRequired,
}
