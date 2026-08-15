using SmartQr.Common.Domain.Codes.Core.Enums;
using SmartQr.Domain.Codes.Core.Enums;
using SmartQr.Domain.Codes.Rules.Models;
using WoW.Two.Sdk.Backend.Beta.Codes.Models.Style;

namespace SmartQr.Api.Requests.Codes;

/// <summary>Represents the stateless preview request body, rendered live with no persistence.</summary>
public sealed record PreviewCodeApiRequest
{
    /// <summary>Gets how the symbol resolves — static bakes the rule's content, dynamic a sample short link.</summary>
    public required ContentMode Mode { get; init; }

    /// <summary>Gets the routing rules whose content the preview bakes.</summary>
    public required IReadOnlyList<CodeRuleValueObject> Rules { get; init; }

    /// <summary>Gets the symbology to render — <c>QrCode</c> is styled, any other format a plain barcode.</summary>
    public BarcodeFormat? BarcodeFormat { get; init; }

    /// <summary>Gets the style to render with — the same whole block a save takes.</summary>
    public required StyleApiRequest Style { get; init; }

    /// <summary>Gets the supplied <see cref="BarcodeFormat"/>, defaulting to QR when absent.</summary>
    public BarcodeFormat ResolveSymbology() => BarcodeFormat ?? SmartQr.Domain.Codes.Core.Enums.BarcodeFormat.QrCode;
}

/// <summary>Provides mapping for <see cref="PreviewCodeApiRequest"/>.</summary>
public static class PreviewCodeApiRequestExtensions
{
    /// <summary>Maps the style block to a <see cref="StyleSpec"/>.</summary>
    /// <param name="request">The preview request whose style block is projected.</param>
    public static StyleSpec ToStyleSpec(this PreviewCodeApiRequest request) => request.Style.ToStyleSpec();
}
