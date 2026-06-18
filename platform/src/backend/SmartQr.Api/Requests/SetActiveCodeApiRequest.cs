using SmartQr.Api.Application.Codes.Core.Commands;

namespace SmartQr.Api.Requests;

/// <summary>Represents the set-active-code request body.</summary>
public sealed record SetActiveCodeApiRequest
{
    /// <summary>Gets whether the code should resolve.</summary>
    public required bool IsActive { get; init; }
}

/// <summary>Provides mapping for <see cref="SetActiveCodeApiRequest"/>.</summary>
public static class SetActiveCodeApiRequestExtensions
{
    /// <summary>Maps the request to its <see cref="CodeSetActiveCommand"/>.</summary>
    public static CodeSetActiveCommand ToCommand(this SetActiveCodeApiRequest request, Guid id, Guid userId)
    {
        var command = new CodeSetActiveCommand
        {
            Id = id,
            UserId = userId,
            IsActive = request.IsActive,
        };

        return command;
    }
}
