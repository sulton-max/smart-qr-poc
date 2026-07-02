namespace SmartQr.Application.Codes.Core.Models;

/// <summary>Outcome of updating a code.</summary>
public abstract record CodeUpdateResult
{
    private CodeUpdateResult() { }

    /// <summary>Updated successfully.</summary>
    public sealed record Success(CodeDto Code) : CodeUpdateResult;
}
