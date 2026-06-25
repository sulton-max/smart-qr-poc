using WoW.Two.Sdk.Backend.Beta.Codes.Models;
using WoW.Two.Sdk.Backend.Beta.Codes.Models.Style;

namespace SmartQr.Tests.Unit;

/// <summary>Proves the <see cref="StyleSpec"/> ↔ <c>StyleJson</c> round-trip and the forgiving fallback (forward-compat with deferred persistence).</summary>
public class StyleSpecJsonTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("{}")]
    [InlineData("not json")]
    public void Deserialize_blank_or_malformed_falls_back_to_default(string? json)
    {
        var spec = StyleSpecJson.Deserialize(json);

        Assert.Equal(StyleSpec.Default, spec);
    }

    [Fact]
    public void Round_trips_a_styled_spec_through_camelcase_json()
    {
        var original = StyleSpec.Default with
        {
            ForegroundColor = "#112233",
            BackgroundColor = "#445566",
            TransparentBackground = true,
            EccLevel = EccLevel.H,
            QuietZoneModules = 6,
            Logo = new LogoSpec { DataUrl = "data:image/png;base64,AAAA", SizeRatio = 0.3 },
        };

        var json = StyleSpecJson.Serialize(original);
        var restored = StyleSpecJson.Deserialize(json);

        Assert.Equal(original, restored);
        Assert.Contains("foregroundColor", json);        // camelCase
        Assert.Contains("\"eccLevel\":\"H\"", json);      // string enum
    }
}
