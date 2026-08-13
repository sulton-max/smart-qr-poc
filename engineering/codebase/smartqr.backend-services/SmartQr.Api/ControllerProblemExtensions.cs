using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using WoW.Two.Sdk.Backend.Beta.Foundation.Errors;
using WoW.Two.Sdk.Backend.Beta.Web.ErrorMapping;
using WoW.Two.Sdk.Backend.Beta.Web.ExceptionHandling;

namespace SmartQr.Api;

/// <summary>Renders an <see cref="AppError"/> failure arm as an RFC 9457 ProblemDetails response.</summary>
internal static class ControllerProblemExtensions
{
    /// <summary>Builds the RFC 9457 ProblemDetails result for <paramref name="error"/>.</summary>
    public static IActionResult ToProblem(this ControllerBase controller, AppError error)
    {
        var http = controller.HttpContext;
        var mapper = http.RequestServices.GetRequiredService<IErrorHttpStatusCodeMapper>();
        var resolver = http.RequestServices.GetRequiredService<IErrorMessageResolver>();
        var problem = AppErrorProblemDetailsFactory.Create(error, http, mapper, resolver);

        return new ObjectResult(problem) { StatusCode = problem.Status };
    }
}
