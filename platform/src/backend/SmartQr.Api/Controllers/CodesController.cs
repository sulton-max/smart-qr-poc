using Microsoft.AspNetCore.Mvc;
using SmartQr.Api.Application.Codes.Core.Commands;
using SmartQr.Api.Application.Codes.Core.Models;
using SmartQr.Api.Application.Codes.Core.Queries;
using SmartQr.Api.Application.Codes.Core.Services;
using SmartQr.Api.Application.Identity.Core.Services;
using SmartQr.Api.Requests;
using SmartQr.Common.Domain.Codes.Enums;
using SmartQr.Common.Models;
using WoW.Two.Sdk.Backend.Beta.Mediator;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Api.Controllers;

/// <summary>Manages codes.</summary>
[ApiController]
[Route("api/codes")]
public sealed class CodesController(
    ISender sender,
    ICodeRepository repository,
    ICodeImageService imageService,
    ICurrentUser currentUser) : ControllerBase
{
    /// <summary>Creates a code.</summary>
    [HttpPost]
    [ProducesResponseType<ApiResponse<CodeDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
    public async Task<IActionResult> Create([FromBody] CreateCodeRequest request, CancellationToken ct)
    {
        if (currentUser.Id is not { } userId)
            return Unauthorized();

        var command = new CodeCreateCommand
        {
            UserId = userId,
            Name = request.Name,
            CodeType = request.CodeType,
            BarcodeFormat = request.BarcodeFormat,
            FallbackUrl = request.FallbackUrl,
            Rules = request.Rules
                .Select(r => new RuleDto
                {
                    Order = r.Order,
                    ConditionType = r.ConditionType,
                    ConditionValue = r.ConditionValue,
                    Destination = r.Destination,
                })
                .ToList(),
        };

        var result = await sender.SendAsync(command, ct);

        return result.Match<CodeCreateResult.Success, CodeCreateResult.Failure, IActionResult>(
            ok => Ok(ApiResponse<CodeDto>.Ok(ok.Data.Code)),
            fail => Problem(detail: fail.Error.ErrorMessage, statusCode: ApiResults.ToStatusCode(fail.Error.Category)));
    }

    /// <summary>Gets a code by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ApiResponse<CodeDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        if (currentUser.Id is not { } userId)
            return Unauthorized();

        var result = await sender.SendAsync(new CodeGetByIdQuery { Id = id, UserId = userId }, ct);

        return result.Match<CodeGetByIdResult.Success, CodeGetByIdResult.Failure, IActionResult>(
            ok => Ok(ApiResponse<CodeDto>.Ok(ok.Data.Code)),
            fail => Problem(detail: fail.Error.ErrorMessage, statusCode: ApiResults.ToStatusCode(fail.Error.Category)));
    }

    /// <summary>Gets all of the caller's codes. Optional <c>?q=</c> filters case-insensitively on name or fallback URL.</summary>
    [HttpGet]
    [ProducesResponseType<ApiResponse<IReadOnlyList<CodeDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Get(CancellationToken ct, [FromQuery] string? q = null)
    {
        if (currentUser.Id is not { } userId)
            return Unauthorized();

        var result = await sender.SendAsync(new CodeListQuery { UserId = userId, Q = q }, ct);

        return result.Match<CodeListResult.Success, CodeListResult.Failure, IActionResult>(
            ok => Ok(ApiResponse<IReadOnlyList<CodeDto>>.Ok(ok.Data.Codes)),
            fail => Problem(detail: fail.Error.ErrorMessage, statusCode: ApiResults.ToStatusCode(fail.Error.Category)));
    }

    /// <summary>Updates a code by id — replaces editable fields and the whole rule set, keeping the immutable slug.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<ApiResponse<CodeDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateById(Guid id, [FromBody] UpdateCodeRequest request, CancellationToken ct)
    {
        if (currentUser.Id is not { } userId)
            return Unauthorized();

        var command = new CodeUpdateCommand
        {
            Id = id,
            UserId = userId,
            Name = request.Name,
            CodeType = request.CodeType,
            BarcodeFormat = request.BarcodeFormat,
            FallbackUrl = request.FallbackUrl,
            Rules = request.Rules
                .Select(r => new RuleDto
                {
                    Order = r.Order,
                    ConditionType = r.ConditionType,
                    ConditionValue = r.ConditionValue,
                    Destination = r.Destination,
                })
                .ToList(),
        };

        var result = await sender.SendAsync(command, ct);

        return result.Match<CodeUpdateResult.Success, CodeUpdateResult.Failure, IActionResult>(
            ok => Ok(ApiResponse<CodeDto>.Ok(ok.Data.Code)),
            fail => Problem(detail: fail.Error.ErrorMessage, statusCode: ApiResults.ToStatusCode(fail.Error.Category)));
    }

    /// <summary>Sets a code's active state by id — enables or disables it.</summary>
    [HttpPatch("{id:guid}/active")]
    [ProducesResponseType<ApiResponse<CodeDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetActiveById(Guid id, [FromBody] SetActiveRequest request, CancellationToken ct)
    {
        if (currentUser.Id is not { } userId)
            return Unauthorized();

        var command = new CodeSetActiveCommand { Id = id, UserId = userId, IsActive = request.IsActive };

        var result = await sender.SendAsync(command, ct);

        return result.Match<CodeSetActiveResult.Success, CodeSetActiveResult.Failure, IActionResult>(
            ok => Ok(ApiResponse<CodeDto>.Ok(ok.Data.Code)),
            fail => Problem(detail: fail.Error.ErrorMessage, statusCode: ApiResults.ToStatusCode(fail.Error.Category)));
    }

    /// <summary>Deletes a code by id, its rules cascading.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteById(Guid id, CancellationToken ct)
    {
        if (currentUser.Id is not { } userId)
            return Unauthorized();

        var result = await sender.SendAsync(new CodeDeleteCommand { Id = id, UserId = userId }, ct);

        return result.Match<CodeDeleteResult.Success, CodeDeleteResult.Failure, IActionResult>(
            NoContent,
            fail => Problem(detail: fail.Error.ErrorMessage, statusCode: ApiResults.ToStatusCode(fail.Error.Category)));
    }

    /// <summary>Gets a code's rendered image by id. <c>?format=svg|png</c>.</summary>
    [HttpGet("{id:guid}/image")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetImageById(Guid id, CancellationToken ct, [FromQuery] string format = "svg")
    {
        if (currentUser.Id is not { } userId)
            return Unauthorized();

        var code = await repository.GetByIdForUserAsync(id, userId, ct);
        if (code is null)
            return NotFound();

        var imageFormat = format.Equals("png", StringComparison.OrdinalIgnoreCase)
            ? ImageFormat.Png
            : ImageFormat.Svg;

        var rendered = imageService.Render(code, imageFormat);
        return File(rendered.Content, rendered.ContentType);
    }
}
