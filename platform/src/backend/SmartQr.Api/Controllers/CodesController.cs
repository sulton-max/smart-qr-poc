using Microsoft.AspNetCore.Mvc;
using SmartQr.Api.Application.Codes.Core.Commands;
using SmartQr.Api.Application.Codes.Core.Models;
using SmartQr.Api.Application.Codes.Core.Queries;
using SmartQr.Api.Application.Codes.Core.Services;
using SmartQr.Api.Application.Identity.Core.Services;
using SmartQr.Api.Requests;
using SmartQr.Common.Domain.Codes.Enums;
using SmartQr.Common.Mediator;
using SmartQr.Common.Models;

namespace SmartQr.Api.Controllers;

/// <summary>Management API for dynamic codes — create, read, list, and render images. Every action is scoped to the calling user.</summary>
[ApiController]
[Route("api/codes")]
public class CodesController(
    IMediator mediator,
    ICodeRepository repository,
    ICodeImageService imageService,
    ICurrentUser currentUser) : ControllerBase
{
    /// <summary>Creates a dynamic code with an optional ordered rule set, owned by the calling user.</summary>
    [HttpPost]
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

        var result = await mediator.SendAsync(command, ct);

        return result is ApplicationResult<CodeCreateResult.Success, CodeCreateResult.Failure>.Success success
            ? Ok(ApiResponse<CodeDto>.Ok(success.Data.Code))
            : Problem(
                detail: (result as ApplicationResult<CodeCreateResult.Success, CodeCreateResult.Failure>.Failure)?.Error.ErrorMessage,
                statusCode: StatusCodes.Status500InternalServerError);
    }

    /// <summary>Returns one of the caller's codes with its rules (401 if anonymous, 404 if not theirs).</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        if (currentUser.Id is not { } userId)
            return Unauthorized();

        var result = await mediator.SendAsync(new CodeGetByIdQuery { Id = id, UserId = userId }, ct);

        if (result is ApplicationResult<CodeGetByIdResult.Success, CodeGetByIdResult.Failure>.Success success)
            return Ok(ApiResponse<CodeDto>.Ok(success.Data.Code));

        var failure = (result as ApplicationResult<CodeGetByIdResult.Success, CodeGetByIdResult.Failure>.Failure)?.Error;
        return failure?.NotFound == true
            ? NotFound()
            : Problem(detail: failure?.ErrorMessage, statusCode: StatusCodes.Status500InternalServerError);
    }

    /// <summary>Lists the caller's codes, newest first (401 if anonymous). Optional <c>?q=</c> filters case-insensitively on name or fallback URL.</summary>
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct, [FromQuery] string? q = null)
    {
        if (currentUser.Id is not { } userId)
            return Unauthorized();

        var result = await mediator.SendAsync(new CodeListQuery { UserId = userId, Q = q }, ct);

        return result is ApplicationResult<CodeListResult.Success, CodeListResult.Failure>.Success success
            ? Ok(ApiResponse<IReadOnlyList<CodeDto>>.Ok(success.Data.Codes))
            : Problem(
                detail: (result as ApplicationResult<CodeListResult.Success, CodeListResult.Failure>.Failure)?.Error.ErrorMessage,
                statusCode: StatusCodes.Status500InternalServerError);
    }

    /// <summary>Updates one of the caller's codes — replaces editable fields and the whole rule set, keeping the immutable slug (401 if anonymous, 404 if not theirs).</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCodeRequest request, CancellationToken ct)
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

        var result = await mediator.SendAsync(command, ct);

        if (result is ApplicationResult<CodeUpdateResult.Success, CodeUpdateResult.Failure>.Success success)
            return Ok(ApiResponse<CodeDto>.Ok(success.Data.Code));

        var failure = (result as ApplicationResult<CodeUpdateResult.Success, CodeUpdateResult.Failure>.Failure)?.Error;
        return failure?.NotFound == true
            ? NotFound()
            : Problem(detail: failure?.ErrorMessage, statusCode: StatusCodes.Status500InternalServerError);
    }

    /// <summary>Enables or disables one of the caller's codes (401 if anonymous, 404 if not theirs).</summary>
    [HttpPatch("{id:guid}/active")]
    public async Task<IActionResult> SetActive(Guid id, [FromBody] SetActiveRequest request, CancellationToken ct)
    {
        if (currentUser.Id is not { } userId)
            return Unauthorized();

        var command = new CodeSetActiveCommand { Id = id, UserId = userId, IsActive = request.IsActive };

        var result = await mediator.SendAsync(command, ct);

        if (result is ApplicationResult<CodeSetActiveResult.Success, CodeSetActiveResult.Failure>.Success success)
            return Ok(ApiResponse<CodeDto>.Ok(success.Data.Code));

        var failure = (result as ApplicationResult<CodeSetActiveResult.Success, CodeSetActiveResult.Failure>.Failure)?.Error;
        return failure?.NotFound == true
            ? NotFound()
            : Problem(detail: failure?.ErrorMessage, statusCode: StatusCodes.Status500InternalServerError);
    }

    /// <summary>Hard-deletes one of the caller's codes, its rules cascading (401 if anonymous, 404 if not theirs).</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        if (currentUser.Id is not { } userId)
            return Unauthorized();

        var result = await mediator.SendAsync(new CodeDeleteCommand { Id = id, UserId = userId }, ct);

        if (result is ApplicationResult<CodeDeleteResult.Success, CodeDeleteResult.Failure>.Success)
            return NoContent();

        var failure = (result as ApplicationResult<CodeDeleteResult.Success, CodeDeleteResult.Failure>.Failure)?.Error;
        return failure?.NotFound == true
            ? NotFound()
            : Problem(detail: failure?.ErrorMessage, statusCode: StatusCodes.Status500InternalServerError);
    }

    /// <summary>Renders the caller's code image (401 if anonymous, 404 if not theirs). <c>?format=svg|png</c>.</summary>
    [HttpGet("{id:guid}/image")]
    public async Task<IActionResult> GetImage(Guid id, CancellationToken ct, [FromQuery] string format = "svg")
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
