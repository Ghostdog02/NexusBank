using MediatR;
using NexusBank.Domain.Entities;
using NexusBank.Domain.Enums;
using NexusBank.Domain.Repositories;

namespace NexusBank.Application.Users.Commands.CreateClerkUser;

public class CreateClerkUserHandler(IUserRepository users) : IRequestHandler<CreateClerkUserCommand>
{
    public async Task Handle(CreateClerkUserCommand command, CancellationToken ct)
    {
        var existing = await users.GetByClerkUserIdAsync(command.ClerkUserId, ct);
        if (existing is not null) return;

        var now = DateTime.UtcNow;

        var user = new User
        {
            Id = Guid.NewGuid(),
            ClerkUserId = command.ClerkUserId,
            Email = command.Email,
            Status = UserStatus.Active,
            KycStatus = KycStatus.Pending,
            CreatedAt = now,
            UpdatedAt = now
        };

        if (command.Provider is not null && command.ProviderUserId is not null)
        {
            user.Identities.Add(new UserIdentity
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Provider = command.Provider.Value,
                ProviderUserId = command.ProviderUserId,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        await users.AddAsync(user, ct);
    }
}
