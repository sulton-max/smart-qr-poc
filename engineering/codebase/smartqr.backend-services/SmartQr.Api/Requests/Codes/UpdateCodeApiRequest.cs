using SmartQr.Application.Codes.Core.Commands;
using SmartQr.Domain.Codes.Core.Enums;
using SmartQr.Domain.Codes.Rules.Models;

namespace SmartQr.Api.Requests.Codes;

/// <summary>Represents the update-code request body. Mode is absent by design — it is fixed at create, so an edit can never change what the symbol bakes.</summary>
public sealed record UpdateCodeApiRequest
{
    /// <summary>Gets the code's display name.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the rendering symbology.</summary>
    public BarcodeFormat BarcodeFormat { get; init; } = BarcodeFormat.QrCode;

    /// <summary>Gets the replacement routing rules (the whole set), each carrying the content it serves.</summary>
    public required IReadOnlyList<CodeRule> Rules { get; init; }

    /// <summary>Gets the optional style to persist — omitted preserves the code's saved style.</summary>
    public StyleApiRequest? Style { get; init; }
}

/// <summary>Provides mapping for <see cref="UpdateCodeApiRequest"/>.</summary>
public static class UpdateCodeApiRequestExtensions
{
    /// <summary>Maps the request to its <see cref="CodeUpdateCommand"/>.</summary>
    public static CodeUpdateCommand ToCommand(this UpdateCodeApiRequest request, Guid id, Guid userId)
    {
        var command = new CodeUpdateCommand
        {
            Id = id,
            UserId = userId,
            Name = request.Name,
            BarcodeFormat = request.BarcodeFormat,
            Rules = request.Rules,
            Style = request.Style?.ToStyleSpec(),
        };

        return command;
    }
}
