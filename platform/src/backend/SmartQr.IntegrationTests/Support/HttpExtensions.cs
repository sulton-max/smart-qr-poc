using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace SmartQr.IntegrationTests.Support;

/// <summary>Compact JSON request/response helpers for the E2E tests.</summary>
public static class HttpExtensions
{
    /// <summary>Serializes <paramref name="body"/> with the API JSON conventions (camelCase and string enums).</summary>
    public static StringContent AsJson(this object body)
    {
        var json = JsonSerializer.Serialize(body, TestJson.Options);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    /// <summary>POST a JSON body.</summary>
    public static Task<HttpResponseMessage> PostJsonAsync(this HttpClient client, string url, object body)
        => client.PostAsync(url, body.AsJson());

    /// <summary>PUT a JSON body.</summary>
    public static Task<HttpResponseMessage> PutJsonAsync(this HttpClient client, string url, object body)
        => client.PutAsync(url, body.AsJson());

    /// <summary>PATCH a JSON body.</summary>
    public static Task<HttpResponseMessage> PatchJsonAsync(this HttpClient client, string url, object body)
        => client.PatchAsync(url, body.AsJson());

    /// <summary>Reads the response body as an <see cref="ApiEnvelope{T}"/> and returns its <c>data</c> payload.</summary>
    public static async Task<T> ReadEnvelopeAsync<T>(this HttpResponseMessage response)
    {
        var envelope = await response.Content.ReadFromJsonAsync<ApiEnvelope<T>>(TestJson.Options);
        return envelope is { Data: { } data }
            ? data
            : throw new InvalidOperationException($"Response had no `data` payload of type {typeof(T).Name}.");
    }
}

/// <summary>Builders for the JSON request bodies the codes endpoints accept.</summary>
public static class CodeRequests
{
    /// <summary>A create/update body. <paramref name="rules"/> is the ordered rule set; pass <c>[]</c> for none.</summary>
    public static object Code(string name, string fallbackUrl, IEnumerable<object>? rules = null) => new
    {
        name,
        codeType = "Qr",
        barcodeFormat = "QrCode",
        fallbackUrl,
        rules = rules?.ToArray() ?? [],
    };

    /// <summary>A single routing rule body.</summary>
    public static object Rule(int order, string conditionType, string? conditionValue, string destination) => new
    {
        order,
        conditionType,
        conditionValue,
        destination,
    };

    /// <summary>An iOS device rule (matches <c>DeviceType.Ios</c>).</summary>
    public static object IosRule(string destination, int order = 1)
        => Rule(order, "Device", "Ios", destination);
}
