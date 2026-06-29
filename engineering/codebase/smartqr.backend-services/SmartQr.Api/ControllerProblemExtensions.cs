using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using WoW.Two.Sdk.Backend.Beta.Foundation.Errors;
using WoW.Two.Sdk.Backend.Beta.Web.ErrorMapping;
using WoW.Two.Sdk.Backend.Beta.Web.ExceptionHandling;

namespace SmartQr.Api;

/// <summary>
/// Renders an <see cref="AppError"/> failure arm as an RFC 9457 ProblemDetails response via the SDK's shared
/// factory + status mapper — the single app-side error→HTTP seam (replaces the retired <c>ApiResults</c> map).
/// </summary>
internal static class ControllerProblemExtensions
{
    /// <summary>Builds the ProblemDetails for <paramref name="error"/> using the registered SDK mapper + message resolver.</summary>
    public static IActionResult ToProblem(this ControllerBase controller, AppError error)
    {
        var http = controller.HttpContext;
        var mapper = http.RequestServices.GetRequiredService<IErrorHttpStatusCodeMapper>();
        var resolver = http.RequestServices.GetRequiredService<IErrorMessageResolver>();
        var problem = AppErrorProblemDetailsFactory.Create(error, http, mapper, resolver);

        return new ObjectResult(problem) { StatusCode = problem.Status };
    }
}
