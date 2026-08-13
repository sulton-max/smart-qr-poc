using SmartQr.Application.Codes.Content.Validators;
using SmartQr.Common.Domain.Codes.Content.Wifi.Enums;
using SmartQr.Domain.Codes.Content.Wifi.Models;

namespace SmartQr.Tests.Unit;

/// <summary>Proves the password rule is gated on the encryption scheme, in both directions.</summary>
/// <remarks>The trailing <c>When</c> applies to every validator in that <c>RuleFor</c>.</remarks>
public sealed class WifiContentValidatorTests
{
    private readonly WifiContentValidator _validator = new();

    private static WifiContentValueObject Network(WifiEncryption encryption, string? password = null) =>
        new() { Ssid = "Cafe", Encryption = encryption, Password = password };

    [Fact]
    public void Open_network_needs_no_password()
    {
        var result = _validator.Validate(Network(WifiEncryption.None));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(WifiEncryption.Wpa)]
    [InlineData(WifiEncryption.Wep)]
    public void Secured_network_requires_a_password(WifiEncryption encryption)
    {
        var result = _validator.Validate(Network(encryption));

        Assert.Contains(result.Errors, failure => failure.PropertyName == nameof(WifiContentValueObject.Password));
    }

    [Fact]
    public void Ssid_is_required()
    {
        var result = _validator.Validate(new WifiContentValueObject
        {
            Ssid = string.Empty,
            Encryption = WifiEncryption.None,
        });

        Assert.Contains(result.Errors, failure => failure.PropertyName == nameof(WifiContentValueObject.Ssid));
    }

    [Fact]
    public void Open_network_accepts_a_password_and_the_encoder_drops_it()
    {
        var network = Network(WifiEncryption.None, "ignored");

        Assert.True(_validator.Validate(network).IsValid);
        Assert.DoesNotContain("ignored", network.Encode());
    }
}
