namespace SmartQr.Application.Codes.Core.Models;

/// <summary>Represents the outcome of creating a code.</summary>
public abstract record CodeCreateResult
{
    private CodeCreateResult() { }

    /// <summary>Created successfully.</summary>
    public sealed record Success(CodeDto Code) : CodeCreateResult;
}
