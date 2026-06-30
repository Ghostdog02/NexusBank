using Microsoft.EntityFrameworkCore;
using NexusBank.Domain.Entities;
using NexusBank.Domain.Repositories;

namespace NexusBank.Infrastructure.Persistence.Repositories;

public class UserRepository(NexusDbContext db) : IUserRepository
{
    public Task<User?> GetByClerkUserIdAsync(string clerkUserId, CancellationToken ct = default)
        => db.Users
             .Include(u => u.Profile)
             .Include(u => u.Identities)
             .FirstOrDefaultAsync(u => u.ClerkUserId == clerkUserId, ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await db.Users.AddAsync(user, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        db.Users.Update(user);
        await db.SaveChangesAsync(ct);
    }
}
