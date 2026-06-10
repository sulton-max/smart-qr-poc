using Microsoft.Extensions.DependencyInjection;
using SmartQr.Codes.Logo;
using SmartQr.Codes.Rendering;

namespace SmartQr.Codes;

/// <summary>DI registration for the code generation library.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Registers the QR/barcode renderers and logo compositor as singletons (stateless, thread-safe).</summary>
    public static IServiceCollection AddSmartQrCodes(this IServiceCollection services)
    {
        services.AddSingleton<ILogoCompositor, ImageSharpLogoCompositor>();
        services.AddSingleton<IQrCodeRenderer, QrCodeRenderer>();
        services.AddSingleton<IBarcodeRenderer, BarcodeRenderer>();
        services.AddSingleton<ICodeRenderer, CodeRenderer>();
        return services;
    }
}
