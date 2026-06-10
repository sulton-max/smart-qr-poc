using Microsoft.Extensions.Logging.Abstractions;
using SmartQr.Api.Application.Codes.Core.Commands;
using SmartQr.Api.Application.Codes.Core.Models;
using SmartQr.Api.Infrastructure.Codes.CommandHandlers;
using SmartQr.Api.Infrastructure.Codes.Services;
using SmartQr.Api.Persistence.Repositories;
using SmartQr.Api.Settings;
using SmartQr.Common.Domain.Codes.Enums;
using SmartQr.Common.Mediator;

namespace SmartQr.Tests;

/// <summary>Full create-command path: handler → repository → SQLite, returning the API DTO.</summary>
public class CodeCreateHandlerTests
{
    [Fact]
    public async Task Create_persists_code_and_returns_dto_with_short_url()
    {
        using var db = new SqliteTestDb();
        var settings = new ApiSettings { RedirectBaseUrl = "https://r.smartqr.test" };

        var handler = new CodeCreateCommandHandler(
            new CodeRepository(db.NewContext()),
            new SlugGenerator(),
            settings,
            NullLogger<CodeCreateCommandHandler>.Instance);

        var command = new CodeCreateCommand
        {
            OwnerId = Guid.NewGuid(),
            Name = "App download",
            FallbackUrl = "https://site.example",
            Rules =
            [
                new RuleDto { Order = 1, ConditionType = RuleConditionType.Device, ConditionValue = "Ios", Destination = "https://apple.example" },
            ],
        };

        var result = await handler.Handle(command, default);

        var success = Assert.IsType<ApplicationResult<CodeCreateResult.Success, CodeCreateResult.Failure>.Success>(result);
        var dto = success.Data.Code;

        Assert.StartsWith("https://r.smartqr.test/", dto.ShortUrl);
        Assert.EndsWith(dto.Slug, dto.ShortUrl);
        Assert.Single(dto.Rules);

        // Persisted and retrievable from a fresh context.
        var loaded = await new CodeRepository(db.NewContext()).GetByIdAsync(dto.Id, default);
        Assert.NotNull(loaded);
        Assert.Equal(dto.Slug, loaded!.Slug);
    }
}
