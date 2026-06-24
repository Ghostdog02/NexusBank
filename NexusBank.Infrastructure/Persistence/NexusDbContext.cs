using Microsoft.EntityFrameworkCore;
using NexusBank.Domain.Entities;

namespace NexusBank.Infrastructure.Persistence;

public class NexusDbContext : DbContext
{
    public NexusDbContext(DbContextOptions<NexusDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();

    public DbSet<UserIdentity> UserIdentities => Set<UserIdentity>();

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NexusDbContext).Assembly);
    }
}
