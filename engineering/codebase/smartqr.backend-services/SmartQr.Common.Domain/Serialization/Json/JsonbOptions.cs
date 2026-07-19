using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace SmartQr.Common.Domain.Serialization.Json;

/// <summary>Builds the serializer options a jsonb column is read and written with.</summary>
public static class JsonbOptions
{
    /// <summary>
    /// Creates options matching the wire shape — Web defaults (camelCase properties) plus camelCase string enums — so a
    /// persisted document and its API representation never drift. Discriminators are read order-tolerantly, since
    /// Postgres jsonb does not preserve object key order and may return the discriminator after the payload.
    /// </summary>
    /// <param name="unionModifiers">Subtype bindings for any polymorphic bases the document contains, from <see cref="SubtypeRegistryJsonExtensions.ToJsonModifier{TBase,TKind}"/>.</param>
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
