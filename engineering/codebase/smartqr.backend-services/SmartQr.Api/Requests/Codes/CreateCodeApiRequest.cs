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

    /// <summary>Gets the code's rendering symbology.</summary>
    public BarcodeFormat BarcodeFormat { get; init; } = BarcodeFormat.QrCode;

    /// <summary>Gets the kind of content every rule carries.</summary>
    public required CodeContentType ContentType { get; init; }

    /// <summary>Gets how the code's symbol resolves.</summary>
    public required ContentMode Mode { get; init; }

    /// <summary>Gets the routing rules, each carrying the content it serves.</summary>
    public required IReadOnlyList<CodeRuleValueObject> Rules { get; init; }

    /// <summary>Gets the style the code renders with.</summary>
    /// <remarks>Send the whole block; the server applies no default.</remarks>
    public required StyleApiRequest Style { get; init; }
}

/// <summary>Extends <see cref="CreateCodeApiRequest"/> for command mapping.</summary>
public static class CreateCodeApiRequestExtensions
{
    /// <summary>Maps the request to its create command.</summary>
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
