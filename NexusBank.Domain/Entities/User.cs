using NexusBank.Domain.Enums;

namespace NexusBank.Domain.Entities;

public class User
{
    public Guid Id { get; set; }

    public string ClerkUserId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public UserStatus Status { get; set; }

    public KycStatus KycStatus { get; set; }

    public DateTime? EmailVerifiedAt { get; set; }

    public DateTime? LastSignInAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? ClosedAt { get; set; }


    public UserProfile? Profile { get; set; }

    public ICollection<UserIdentity> Identities { get; set; } = new List<UserIdentity>();
}
