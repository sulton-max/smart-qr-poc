namespace SmartQr.Api.Application.Codes.Core.Models;

/// <summary>Outcome of fetching a single code.</summary>
public abstract record CodeGetByIdResult
{
    private CodeGetByIdResult() { }

    /// <summary>Found.</summary>
    public sealed record Success(CodeDto Code) : CodeGetByIdResult;
}
