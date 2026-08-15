namespace SmartQr.Application.Codes.Core.Models;

/// <summary>Represents the outcome of listing an owner's codes.</summary>
public abstract record CodeListResult
{
    private CodeListResult() { }

    /// <summary>Listed successfully.</summary>
    public sealed record Success(IReadOnlyList<CodeDto> Codes) : CodeListResult;
}
