using NexusBank.Domain.Entities;

namespace NexusBank.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByClerkUserIdAsync(string clerkUserId, CancellationToken ct = default);
    Task<User?> GetByOnboardingIdAsync(string onboardingId, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task UpdateAsync(User user, CancellationToken ct = default);
}
