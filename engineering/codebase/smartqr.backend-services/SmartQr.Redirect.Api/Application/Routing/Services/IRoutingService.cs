using SmartQr.Domain.Codes.Core.Entities;
using SmartQr.Redirect.Api.Application.Routing.Models;

namespace SmartQr.Redirect.Api.Application.Routing.Services;

/// <summary>Defines the contract for deciding where a scan resolves.</summary>
public interface IRoutingService
{
    /// <summary>Evaluates the code's rules in order, taking the first match.</summary>
    /// <param name="code">The scanned code with its routing rules loaded.</param>
    /// <param name="context">The resolved scan context (device, geo, language, time).</param>
    RoutingResult Evaluate(CodeEntity code, ScanContext context);
}
