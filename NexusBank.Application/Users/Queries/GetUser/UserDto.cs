using NexusBank.Domain.Enums;

namespace NexusBank.Application.Users.Queries.GetUser;

public record UserDto(
    Guid Id,
    string Email,
    UserStatus Status,
    KycStatus KycStatus,
    DateTime? EmailVerifiedAt,
    DateTime? LastSignInAt,
    DateTime CreatedAt
);
