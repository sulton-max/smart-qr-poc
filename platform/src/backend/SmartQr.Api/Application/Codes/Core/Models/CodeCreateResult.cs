namespace SmartQr.Api.Application.Codes.Core.Models;

/// <summary>Outcome of creating a code.</summary>
public abstract record CodeCreateResult
{
    private CodeCreateResult() { }

    /// <summary>Created successfully.</summary>
    public sealed record Success(CodeDto Code) : CodeCreateResult;
}
