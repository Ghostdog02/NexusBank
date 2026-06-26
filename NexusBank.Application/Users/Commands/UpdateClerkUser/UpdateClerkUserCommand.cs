using MediatR;

namespace NexusBank.Application.Users.Commands.UpdateClerkUser;

public record UpdateClerkUserCommand(
    string ClerkUserId,
    string Email,
    DateTime? EmailVerifiedAt,
    DateTime? LastSignInAt
) : IRequest;
