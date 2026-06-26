using MediatR;
using NexusBank.Domain.Enums;
using NexusBank.Domain.Repositories;

namespace NexusBank.Application.Users.Commands.CloseClerkUser;

public class CloseClerkUserHandler(IUserRepository users) : IRequestHandler<CloseClerkUserCommand>
{
    public async Task Handle(CloseClerkUserCommand command, CancellationToken ct)
    {
        var user = await users.GetByClerkUserIdAsync(command.ClerkUserId, ct);
        if (user is null) return;

        var now = DateTime.UtcNow;

        user.Status = UserStatus.Closed;
        user.ClosedAt = now;
        user.UpdatedAt = now;

        await users.UpdateAsync(user, ct);
    }
}
