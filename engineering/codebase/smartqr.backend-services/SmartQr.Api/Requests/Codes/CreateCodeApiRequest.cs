using SmartQr.Application.Codes.Core.Commands;
using SmartQr.Common.Domain.Codes.Core.Enums;
using SmartQr.Domain.Codes.Core.Enums;
using SmartQr.Domain.Codes.Rules.Models;

namespace SmartQr.Api.Requests.Codes;

/// <summary>Represents the create-code request body.</summary>
public sealed record CreateCodeApiRequest
{
    /// <summary>Gets the code's display name.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the rendering symbology.</summary>
    public BarcodeFormat BarcodeFormat { get; init; } = BarcodeFormat.QrCode;

    /// <summary>Gets the kind of content every rule carries — the builder's content-type choice. Validated against the rules it ships with.</summary>
    public required CodeContentType ContentType { get; init; }

    /// <summary>Gets how the symbol resolves — a baked payload (static) or the redirect short link (dynamic). Fixed for the life of the code.</summary>
    public required ContentMode Mode { get; init; }

    /// <summary>Gets the routing rules, each carrying the content it serves. At least one is required; every rule must carry the same content type.</summary>
    public required IReadOnlyList<CodeRule> Rules { get; init; }

    /// <summary>Gets the style the code renders with — a code always has one, so the client sends the whole block rather than leaning on a server default.</summary>
    public required StyleApiRequest Style { get; init; }
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
            BarcodeFormat = request.BarcodeFormat,
            ContentType = request.ContentType,
            Mode = request.Mode,
            Rules = request.Rules,
            Style = request.Style.ToStyleSpec(),
        };

        return command;
    }
}
