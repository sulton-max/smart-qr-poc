using System.Security.Cryptography;
using SmartQr.Application.Codes.Core.Services;

namespace SmartQr.Infrastructure.Codes.Core.Services;

/// <summary>Provides cryptographically random base62 slugs.</summary>
public sealed class SlugGenerator : ISlugGenerator
{
    private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const int Length = 7;

    /// <inheritdoc />
    public string Next()
    {
        Span<byte> bytes = stackalloc byte[Length];
        RandomNumberGenerator.Fill(bytes);

        var chars = new char[Length];
        for (var i = 0; i < Length; i++)
            chars[i] = Alphabet[bytes[i] % Alphabet.Length];

        return new string(chars);
    }
}
