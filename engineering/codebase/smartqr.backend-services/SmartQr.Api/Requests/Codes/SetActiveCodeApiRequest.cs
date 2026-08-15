using SmartQr.Application.Codes.Core.Commands;

namespace SmartQr.Api.Requests.Codes;

/// <summary>Represents the set-active-code request body.</summary>
public sealed record SetActiveCodeApiRequest
{
    /// <summary>Gets whether the code should resolve.</summary>
    public required bool IsActive { get; init; }
}

/// <summary>Extends <see cref="SetActiveCodeApiRequest"/> for command mapping.</summary>
public static class SetActiveCodeApiRequestExtensions
{
    /// <summary>Maps the request to its set-active command.</summary>
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
