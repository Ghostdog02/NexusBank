namespace NexusBank.Application.Users.Queries.GetCurrentUser;

public record CurrentUserDto(
    string Email,
    string? PhoneNumber,
    DateOnly DateOfBirth,
    string FirstName,
    string LastName,
    string NationalId,
    string Country
);
