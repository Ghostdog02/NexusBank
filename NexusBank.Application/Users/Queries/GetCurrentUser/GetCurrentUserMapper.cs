using NexusBank.Domain.Entities;

namespace NexusBank.Application.Users.Queries.GetCurrentUser;

public static class GetCurrentUserMapper
{
    public static CurrentUserDto ToDto(string email, UserProfile profile) => new(
        email,
        profile.PhoneNumber,
        profile.DateOfBirth,
        profile.FirstName,
        profile.LastName,
        profile.NationalId,
        profile.Country
    );
}
