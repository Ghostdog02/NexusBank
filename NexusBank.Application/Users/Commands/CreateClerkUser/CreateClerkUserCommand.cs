using MediatR;
using NexusBank.Domain.Enums;

namespace NexusBank.Application.Users.Commands.CreateClerkUser;

public record CreateClerkUserCommand(
    string ClerkUserId,
    string Email,
    IdentityProvider? Provider,
    string? ProviderUserId
) : IRequest;
