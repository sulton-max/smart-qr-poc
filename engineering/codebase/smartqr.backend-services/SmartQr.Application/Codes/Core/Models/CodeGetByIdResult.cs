namespace SmartQr.Application.Codes.Core.Models;

/// <summary>Represents the outcome of fetching a single code.</summary>
public abstract record CodeGetByIdResult
{
    private CodeGetByIdResult() { }

    /// <summary>Found.</summary>
    public sealed record Success(CodeDto Code) : CodeGetByIdResult;
}
