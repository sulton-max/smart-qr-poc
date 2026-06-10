using System.Security.Cryptography;
using SmartQr.Api.Application.Codes.Core.Services;

namespace SmartQr.Api.Infrastructure.Codes.Services;

/// <summary>Cryptographically-random base62 slug generator (7 chars ≈ 3.5 trillion combinations).</summary>
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
