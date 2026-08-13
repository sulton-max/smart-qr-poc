using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace SmartQr.Common.Domain.Serialization.Json;

/// <summary>Builds the serializer options a jsonb column is read and written with.</summary>
public static class JsonbOptions
{
    /// <summary>Creates options matching the wire shape — Web defaults plus camelCase string enums.</summary>
    /// <param name="unionModifiers">Subtype bindings for the polymorphic bases the document contains.</param>
    public static JsonSerializerOptions Create(params Action<JsonTypeInfo>[] unionModifiers)
    {
        var resolver = new DefaultJsonTypeInfoResolver();
        foreach (var modifier in unionModifiers)
            resolver.Modifiers.Add(modifier);

        return new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            AllowOutOfOrderMetadataProperties = true,
            TypeInfoResolver = resolver,
        };
    }
}
