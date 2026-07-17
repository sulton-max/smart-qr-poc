using SmartQr.Application.Codes.Core.Commands;
using SmartQr.Domain.Codes.Content;
using SmartQr.Domain.Codes.Core.Enums;

namespace SmartQr.Api.Requests.Codes;

/// <summary>Represents the update-code request body.</summary>
public sealed record UpdateCodeApiRequest
{
    /// <summary>Gets the code's display name.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the rendering symbology.</summary>
    public BarcodeFormat BarcodeFormat { get; init; } = BarcodeFormat.QrCode;

    /// <summary>Gets the replacement ordered routing rules (the whole set).</summary>
    public IReadOnlyList<RuleApiRequest> Rules { get; init; } = [];

    /// <summary>Gets the optional style to persist — omitted preserves the code's saved style.</summary>
    public StyleApiRequest? Style { get; init; }

    /// <summary>Gets the optional typed content the code carries (polymorphic on <c>type</c>); omitted preserves the code's saved content.</summary>
    public CodeContent? Content { get; init; }
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
            Rules = request.Rules.ToRuleDtos(),
            Style = request.Style?.ToStyleSpec(),
            Content = request.Content,
        };

        return command;
    }
}
