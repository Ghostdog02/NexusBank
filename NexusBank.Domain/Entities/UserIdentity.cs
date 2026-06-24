using NexusBank.Domain.Enums;

namespace NexusBank.Domain.Entities;

public class UserIdentity
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public IdentityProvider Provider { get; set; }

    public string ProviderUserId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }


    public User User { get; set; } = null!;
}
