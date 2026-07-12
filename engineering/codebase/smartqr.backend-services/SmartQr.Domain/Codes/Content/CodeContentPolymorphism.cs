using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using SmartQr.Domain.Codes.Core.Enums;

namespace SmartQr.Domain.Codes.Content;

/// <summary>
/// Wires <see cref="CodeContent"/> polymorphism from the <see cref="CodeContentType"/> enum — the single source of
/// truth. Each concrete content type's discriminator is its <see cref="CodeContent.Type"/> in camelCase, so no
/// per-type magic string or attribute can drift from the enum.
/// </summary>
public static class CodeContentPolymorphism
{
    private const string TypeDiscriminatorPropertyName = "type";

    private static readonly IReadOnlyList<(Type Type, string Discriminator)> DerivedTypes = BuildDerivedTypes();

    /// <summary>A <see cref="DefaultJsonTypeInfoResolver"/> modifier that attaches the polymorphism options to the <see cref="CodeContent"/> base.</summary>
    /// <param name="typeInfo">The type info the resolver is building.</param>
    public static void Configure(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Type != typeof(CodeContent))
        {
            return;
        }

        var polymorphism = new JsonPolymorphismOptions { TypeDiscriminatorPropertyName = TypeDiscriminatorPropertyName };
        foreach (var (type, discriminator) in DerivedTypes)
        {
            polymorphism.DerivedTypes.Add(new JsonDerivedType(type, discriminator));
        }

        typeInfo.PolymorphismOptions = polymorphism;
    }

    private static IReadOnlyList<(Type Type, string Discriminator)> BuildDerivedTypes()
    {
        // Serialize the enum through the wire's string-enum converter so the discriminator honors any
        // per-member override (`[JsonStringEnumMemberName]`) — the enum member stays the single source.
        var enumOptions = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) } };

        return typeof(CodeContent).Assembly.GetTypes()
            .Where(type => type is { IsAbstract: false } && typeof(CodeContent).IsAssignableFrom(type))
            .Select(type =>
            {
                var kind = ((CodeContent)RuntimeHelpers.GetUninitializedObject(type)).Type;
                return (type, JsonSerializer.Serialize(kind, enumOptions).Trim('"'));
            })
            .ToList();
    }
}
