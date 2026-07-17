using WoW.Two.Sdk.Backend.Beta.Codes.Models.Style;
using SmartQr.Domain.Codes.Content;
using SmartQr.Domain.Codes.Core.Enums;

namespace SmartQr.Api.Requests.Codes;

/// <summary>Represents the stateless preview request body, rendered live from <see cref="Style"/> with no persistence.</summary>
public sealed record PreviewCodeApiRequest
{
    /// <summary>Gets the fallback data to encode when <see cref="Content"/> is absent or dynamic — the short link on edit, a sample URL on create.</summary>
    public required string Value { get; init; }

    /// <summary>Gets the optional typed content — when it bakes a payload (<see cref="CodeContent.Encode"/>), the preview encodes it server-side so it matches the saved asset; dynamic/absent content falls back to <see cref="Value"/>.</summary>
    public CodeContent? Content { get; init; }

    /// <summary>Gets the symbology to render — <c>QrCode</c> renders the styled path, any other format renders a plain ZXing barcode.</summary>
    public BarcodeFormat? BarcodeFormat { get; init; }

    /// <summary>Gets the style to render with — <c>required</c>: the builder always sends the full style block, so an absent block fails binding (400).</summary>
    public required StyleApiRequest Style { get; init; }

    /// <summary>Gets the symbology to render — the supplied <see cref="BarcodeFormat"/>, defaulting to QR when absent.</summary>
    public BarcodeFormat ResolveSymbology() => BarcodeFormat ?? SmartQr.Domain.Codes.Core.Enums.BarcodeFormat.QrCode;
}

/// <summary>Provides mapping for <see cref="PreviewCodeApiRequest"/>.</summary>
public static class PreviewCodeApiRequestExtensions
{
    /// <summary>Maps the request's <c>required</c> style block to a <see cref="StyleSpec"/> directly — the wire always carries every field, so there is nothing to default.</summary>
    /// <param name="request">The preview request whose style block is projected.</param>
    public static StyleSpec ToStyleSpec(this PreviewCodeApiRequest request) => request.Style.ToStyleSpec();
}
