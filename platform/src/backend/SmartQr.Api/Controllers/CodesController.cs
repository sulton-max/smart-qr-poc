using Microsoft.AspNetCore.Mvc;
using SmartQr.Api.Application.Codes.Core.Commands;
using SmartQr.Api.Application.Codes.Core.Models;
using SmartQr.Api.Application.Codes.Core.Queries;
using SmartQr.Api.Application.Codes.Core.Services;
using SmartQr.Api.Requests;
using SmartQr.Common.Domain.Codes.Enums;
using SmartQr.Common.Mediator;
using SmartQr.Common.Models;

namespace SmartQr.Api.Controllers;

/// <summary>Management API for dynamic codes — create, read, list, and render images.</summary>
[ApiController]
[Route("api/codes")]
public class CodesController(
    IMediator mediator,
    ICodeRepository repository,
    ICodeImageService imageService) : ControllerBase
{
    // POC stand-in for the authenticated user/workspace until auth lands.
    private static readonly Guid DemoOwner = Guid.Parse("00000000-0000-0000-0000-000000000001");

    /// <summary>Creates a dynamic code with an optional ordered rule set.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCodeRequest request, CancellationToken ct)
    {
        var command = new CodeCreateCommand
        {
            OwnerId = request.OwnerId ?? DemoOwner,
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

    /// <summary>Returns a single code with its rules.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.SendAsync(new CodeGetByIdQuery { Id = id }, ct);

        if (result is ApplicationResult<CodeGetByIdResult.Success, CodeGetByIdResult.Failure>.Success success)
            return Ok(ApiResponse<CodeDto>.Ok(success.Data.Code));

        var failure = (result as ApplicationResult<CodeGetByIdResult.Success, CodeGetByIdResult.Failure>.Failure)?.Error;
        return failure?.NotFound == true
            ? NotFound()
            : Problem(detail: failure?.ErrorMessage, statusCode: StatusCodes.Status500InternalServerError);
    }

    /// <summary>Lists the owner's codes, newest first.</summary>
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] Guid? ownerId, CancellationToken ct)
    {
        var result = await mediator.SendAsync(new CodeListQuery { OwnerId = ownerId ?? DemoOwner }, ct);

        return result is ApplicationResult<CodeListResult.Success, CodeListResult.Failure>.Success success
            ? Ok(ApiResponse<IReadOnlyList<CodeDto>>.Ok(success.Data.Codes))
            : Problem(
                detail: (result as ApplicationResult<CodeListResult.Success, CodeListResult.Failure>.Failure)?.Error.ErrorMessage,
                statusCode: StatusCodes.Status500InternalServerError);
    }

    /// <summary>Renders the code image (the QR/barcode encoding its short URL). <c>?format=svg|png</c>.</summary>
    [HttpGet("{id:guid}/image")]
    public async Task<IActionResult> GetImage(Guid id, CancellationToken ct, [FromQuery] string format = "svg")
    {
        var code = await repository.GetByIdAsync(id, ct);
        if (code is null)
            return NotFound();

        var imageFormat = format.Equals("png", StringComparison.OrdinalIgnoreCase)
            ? ImageFormat.Png
            : ImageFormat.Svg;

        var rendered = imageService.Render(code, imageFormat);
        return File(rendered.Content, rendered.ContentType);
    }
}
