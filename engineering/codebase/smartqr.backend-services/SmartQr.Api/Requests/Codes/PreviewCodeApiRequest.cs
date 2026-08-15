using SmartQr.Common.Domain.Codes.Core.Enums;
using SmartQr.Domain.Codes.Core.Enums;
using SmartQr.Domain.Codes.Rules.Models;
using WoW.Two.Sdk.Backend.Beta.Codes.Models.Style;

namespace SmartQr.Api.Requests.Codes;

/// <summary>Represents the preview request body.</summary>
public sealed record PreviewCodeApiRequest
{
    /// <summary>Gets how the previewed symbol resolves.</summary>
    public required ContentMode Mode { get; init; }

    /// <summary>Gets the routing rules, each carrying the content it serves.</summary>
    public required IReadOnlyList<CodeRuleValueObject> Rules { get; init; }

    /// <summary>Gets the symbology to render.</summary>
    public BarcodeFormat? BarcodeFormat { get; init; }

    /// <summary>Gets the style to render with.</summary>
    public required StyleApiRequest Style { get; init; }

    /// <summary>Gets the supplied symbology, defaulting to QR when absent.</summary>
    public BarcodeFormat ResolveSymbology() => BarcodeFormat ?? SmartQr.Domain.Codes.Core.Enums.BarcodeFormat.QrCode;
}

/// <summary>Extends <see cref="PreviewCodeApiRequest"/> for style-spec mapping.</summary>
public static class PreviewCodeApiRequestExtensions
{
    /// <summary>Maps the style block to its style spec.</summary>
    /// <param name="request">The preview request whose style block is projected.</param>
    public static StyleSpec ToStyleSpec(this PreviewCodeApiRequest request) => request.Style.ToStyleSpec();
}
