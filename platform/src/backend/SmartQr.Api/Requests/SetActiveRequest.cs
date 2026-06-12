namespace SmartQr.Api.Requests;

/// <summary>Inbound shape for enabling or disabling a code (toggles <c>is_active</c> only).</summary>
public sealed record SetActiveRequest
{
    /// <summary>Whether the code should resolve.</summary>
    public required bool IsActive { get; init; }
}
