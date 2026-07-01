using Microsoft.AspNetCore.Authorization;
using NexusBank.Domain.Enums;

namespace NexusBank.Application.Common.Authorization;

public class RoleRequirement(UserRole role) : IAuthorizationRequirement
{
    public UserRole Role { get; } = role;
}
