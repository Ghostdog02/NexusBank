using MediatR;

namespace NexusBank.Application.Users.Commands.CloseClerkUser;

public record CloseClerkUserCommand(string ClerkUserId) : IRequest;
