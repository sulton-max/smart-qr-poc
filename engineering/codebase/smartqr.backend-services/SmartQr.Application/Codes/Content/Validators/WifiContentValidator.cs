using FluentValidation;
using SmartQr.Common.Domain.Codes.Content.Wifi.Enums;
using SmartQr.Domain.Codes.Content.Wifi.Models;

namespace SmartQr.Application.Codes.Content.Validators;

/// <summary>Validates Wi-Fi join credentials.</summary>
public sealed class WifiContentValidator : AbstractValidator<WifiContent>
{
    /// <summary>Builds the wifi-content rules.</summary>
    public WifiContentValidator()
    {
        RuleFor(content => content.Ssid).NotEmpty().WithMessage("Network name is required.");

        // An open network carries no password; every other scheme needs one to be joinable.
        RuleFor(content => content.Password)
            .NotEmpty().WithMessage("Password is required on a secured network.")
            .When(content => content.Encryption is not WifiEncryption.None);
    }
}
