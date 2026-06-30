using MediatR;
using NexusBank.Application.Common.Services;
using NexusBank.Domain.Exceptions;

namespace NexusBank.Application.Users.Queries.GetCurrentUser;

public class GetCurrentUserHandler(ICurrentUserService currentUserService)
    : IRequestHandler<GetCurrentUserQuery, CurrentUserDto>
{
    public async Task<CurrentUserDto> Handle(GetCurrentUserQuery request, CancellationToken ct)
    {
        var user = await currentUserService.GetCurrentUserAsync(ct);

        if (user.Profile is null)
            throw new NotFoundException("User profile not found.");

        return GetCurrentUserMapper.ToDto(user.Email, user.Profile);
    }
}
