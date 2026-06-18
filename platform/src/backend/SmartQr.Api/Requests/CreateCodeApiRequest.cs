using SmartQr.Api.Application.Codes.Core.Commands;
using SmartQr.Api.Application.Codes.Core.Models;
using SmartQr.Common.Domain.Codes.Enums;

namespace SmartQr.Api.Requests;

/// <summary>Represents the create-code request body.</summary>
public sealed record CreateCodeApiRequest
{
    /// <summary>Gets the code's display name.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the high-level kind of code.</summary>
    public CodeType CodeType { get; init; } = CodeType.Qr;

    /// <summary>Gets the rendering symbology.</summary>
    public BarcodeFormat BarcodeFormat { get; init; } = BarcodeFormat.QrCode;

    /// <summary>Gets the default destination when no rule matches.</summary>
    public required string FallbackUrl { get; init; }

    /// <summary>Gets the optional ordered routing rules.</summary>
    public IReadOnlyList<RuleApiRequest> Rules { get; init; } = [];
}

/// <summary>Provides mapping for <see cref="CreateCodeApiRequest"/>.</summary>
public static class CreateCodeApiRequestExtensions
{
    /// <summary>Maps the request to its <see cref="CodeCreateCommand"/>.</summary>
    public static CodeCreateCommand ToCommand(this CreateCodeApiRequest request, Guid userId)
    {
        var command = new CodeCreateCommand
        {
            UserId = userId,
            Name = request.Name,
            CodeType = request.CodeType,
            BarcodeFormat = request.BarcodeFormat,
            FallbackUrl = request.FallbackUrl,
            Rules = request.Rules.ToRuleDtos(),
        };

        return command;
    }
}
