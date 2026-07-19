using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartQr.Common.Domain.Serialization;

/// <summary>
/// The closed set of subtypes of a polymorphic base, each bound to a discriminator enum member. Declared once as a
/// static member of the base it describes; the constructor rejects an incomplete or malformed set, so a missing
/// variant fails at startup rather than on the first request. Serializer-agnostic — bind it with a
/// <c>To{Format}</c> extension (see <see cref="Json.SubtypeRegistryJsonExtensions"/>).
/// </summary>
/// <typeparam name="TBase">The polymorphic base type.</typeparam>
/// <typeparam name="TKind">The enum discriminating the subtypes.</typeparam>
public sealed class SubtypeRegistry<TBase, TKind>
    where TBase : class
    where TKind : struct, Enum
{
    private readonly Dictionary<TKind, Type> _typesByKind;
    private readonly Dictionary<Type, TKind> _kindsByType;

    /// <summary>Creates the registry, validating that the set covers every <typeparamref name="TKind"/> member exactly once.</summary>
    /// <param name="subtypes">Each discriminator member paired with the concrete type it identifies.</param>
    /// <exception cref="ArgumentException">A subtype does not derive from <typeparamref name="TBase"/>, a kind or type repeats, or a <typeparamref name="TKind"/> member has no subtype.</exception>
    public SubtypeRegistry(params (TKind Kind, Type Type)[] subtypes)
    {
        foreach (var (kind, type) in subtypes)
        {
            if (!typeof(TBase).IsAssignableFrom(type))
                throw new ArgumentException($"'{type.Name}' does not derive from '{typeof(TBase).Name}'.", nameof(subtypes));

            if (type.IsAbstract)
                throw new ArgumentException($"'{type.Name}' is abstract and cannot be a subtype.", nameof(subtypes));
        }

        _typesByKind = [];
        _kindsByType = [];

        foreach (var (kind, type) in subtypes)
        {
            if (!_typesByKind.TryAdd(kind, type))
                throw new ArgumentException($"'{kind}' is mapped more than once.", nameof(subtypes));

            if (!_kindsByType.TryAdd(type, kind))
                throw new ArgumentException($"'{type.Name}' is mapped more than once.", nameof(subtypes));
        }

        var missing = Enum.GetValues<TKind>().Where(kind => !_typesByKind.ContainsKey(kind)).ToArray();
        if (missing.Length > 0)
            throw new ArgumentException($"'{typeof(TKind).Name}' members without a subtype: {string.Join(", ", missing)}.", nameof(subtypes));

        Subtypes = subtypes.Select(subtype => (subtype.Kind, subtype.Type, Discriminator: ToDiscriminator(subtype.Kind))).ToArray();
    }

    /// <summary>Every subtype, paired with its discriminator member and the wire token that member serializes to.</summary>
    public IReadOnlyList<(TKind Kind, Type Type, string Discriminator)> Subtypes { get; }

    /// <summary>Resolves the concrete type a discriminator member identifies.</summary>
    /// <param name="kind">The discriminator member.</param>
    public Type TypeOf(TKind kind) => _typesByKind[kind];

    /// <summary>Resolves the discriminator member of an instance's concrete type.</summary>
    /// <param name="instance">The instance to classify.</param>
    public TKind KindOf(TBase instance) => _kindsByType[instance.GetType()];

    // Serialize the member through the wire's string-enum converter so the token honors any
    // [JsonStringEnumMemberName] override — the enum member stays the single source.
    private static string ToDiscriminator(TKind kind)
    {
        var options = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) } };
        return JsonSerializer.Serialize(kind, options).Trim('"');
    }
}
