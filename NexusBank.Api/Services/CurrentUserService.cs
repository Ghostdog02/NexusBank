using System.Security.Claims;
using NexusBank.Application.Common.Services;
using NexusBank.Domain.Entities;
using NexusBank.Domain.Exceptions;
using NexusBank.Domain.Repositories;

namespace NexusBank.Api.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor, IUserRepository userRepository)
    : ICurrentUserService
{
    private User? _cachedUser;

    public async Task<User> GetCurrentUserAsync(CancellationToken ct = default)
    {
        if (_cachedUser is not null)
            return _cachedUser;

        var clerkUserId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("No authenticated user found.");

        _cachedUser = await userRepository.GetByClerkUserIdAsync(clerkUserId, ct)
            ?? throw new NotFoundException("Account not yet created. Please retry in a moment.");

        return _cachedUser;
    }
}
