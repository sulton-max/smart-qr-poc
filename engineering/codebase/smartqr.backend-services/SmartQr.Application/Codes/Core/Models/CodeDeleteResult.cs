namespace SmartQr.Application.Codes.Core.Models;

/// <summary>Represents the outcome of deleting a code.</summary>
public abstract record CodeDeleteResult
{
    private CodeDeleteResult() { }

    /// <summary>Deleted successfully.</summary>
    public sealed record Success : CodeDeleteResult;
}
