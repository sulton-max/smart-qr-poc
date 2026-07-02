namespace SmartQr.Application.Codes.Core.Models;

/// <summary>Outcome of listing a owner's codes.</summary>
public abstract record CodeListResult
{
    private CodeListResult() { }

    /// <summary>Listed successfully.</summary>
    public sealed record Success(IReadOnlyList<CodeDto> Codes) : CodeListResult;
}
