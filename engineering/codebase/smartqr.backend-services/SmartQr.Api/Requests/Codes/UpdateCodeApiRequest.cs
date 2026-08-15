using SmartQr.Application.Codes.Core.Commands;
using SmartQr.Domain.Codes.Core.Enums;
using SmartQr.Domain.Codes.Rules.Models;

namespace SmartQr.Api.Requests.Codes;

/// <summary>Represents the update-code request body.</summary>
public sealed record UpdateCodeApiRequest
{
    /// <summary>Gets the code's display name.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the code's rendering symbology.</summary>
    public BarcodeFormat BarcodeFormat { get; init; } = BarcodeFormat.QrCode;

    /// <summary>Gets the kind of content every rule carries.</summary>
    public required CodeContentType ContentType { get; init; }

    /// <summary>Gets the replacement routing rules, each carrying the content it serves.</summary>
    public required IReadOnlyList<CodeRuleValueObject> Rules { get; init; }

    /// <summary>Gets the replacement style the code renders with.</summary>
    public required StyleApiRequest Style { get; init; }
}

/// <summary>Extends <see cref="UpdateCodeApiRequest"/> for command mapping.</summary>
public static class UpdateCodeApiRequestExtensions
{
    /// <summary>Maps the request to its update command.</summary>
    public static CodeUpdateCommand ToCommand(this UpdateCodeApiRequest request, Guid id, Guid userId)
    {
        var command = new CodeUpdateCommand
        {
            Id = id,
            UserId = userId,
            Name = request.Name,
            BarcodeFormat = request.BarcodeFormat,
            ContentType = request.ContentType,
            Rules = request.Rules,
            Style = request.Style.ToStyleSpec(),
        };

        return command;
    }
}
