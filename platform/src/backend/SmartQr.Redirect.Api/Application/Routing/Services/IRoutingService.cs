using SmartQr.Redirect.Api.Application.Routing.Models;

namespace SmartQr.Redirect.Api.Application.Routing.Services;

/// <summary>Business-rule orchestration — given a config and a scan context, decide where to send the scanner.</summary>
public interface IRoutingService
{
    /// <summary>Evaluates rules top-to-bottom (first match wins), else falls back. Also enforces active/expiry/password.</summary>
    RouteDecision Evaluate(CodeRouteConfig config, ScanContext context);
}
