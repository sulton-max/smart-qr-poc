namespace SmartQr.Api.Application.Codes.Core.Models;

/// <summary>Outcome of enabling or disabling a code.</summary>
public abstract record CodeSetActiveResult
{
    private CodeSetActiveResult() { }

    /// <summary>Toggled successfully.</summary>
    public sealed record Success(CodeDto Code) : CodeSetActiveResult;
}
