using SmartQr.Domain.Codes.Core.Entities;
using SmartQr.Redirect.Api.Application.Routing.Models;

namespace SmartQr.Redirect.Api.Application.Routing.Services;

/// <summary>Business-rule orchestration — given a code and a scan context, decide where to send the scanner.</summary>
public interface IRoutingService
{
    /// <summary>Evaluates the code's rules top-to-bottom (first match wins); no match means the code does not resolve. Also enforces active/expiry.</summary>
    RouteDecision Evaluate(CodeEntity code, ScanContext context);
}
