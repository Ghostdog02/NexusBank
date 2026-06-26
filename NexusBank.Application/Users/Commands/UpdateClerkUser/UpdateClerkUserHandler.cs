using MediatR;
using NexusBank.Domain.Repositories;

namespace NexusBank.Application.Users.Commands.UpdateClerkUser;

public class UpdateClerkUserHandler(IUserRepository users) : IRequestHandler<UpdateClerkUserCommand>
{
    public async Task Handle(UpdateClerkUserCommand command, CancellationToken ct)
    {
        var user = await users.GetByClerkUserIdAsync(command.ClerkUserId, ct);
        if (user is null) return;

        user.Email = command.Email;
        user.EmailVerifiedAt = command.EmailVerifiedAt;
        user.LastSignInAt = command.LastSignInAt;
        user.UpdatedAt = DateTime.UtcNow;

        await users.UpdateAsync(user, ct);
    }
}
