using MediatR;

namespace NexusBank.Application.Users.Commands.CreateSwanAccount;

public record CreateSwanAccountCommand(string ClerkUserId, string Email) : IRequest;
