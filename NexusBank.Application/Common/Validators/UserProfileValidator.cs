using FluentValidation;
using NexusBank.Domain.Entities;

namespace NexusBank.Application.Common.Validators;

public class UserProfileValidator : AbstractValidator<UserProfile>
{
    public UserProfileValidator()
    {
        RuleFor(p => p.FirstName)
            .MaximumLength(100)
            .When(p => p.FirstName is not null);

        RuleFor(p => p.LastName)
            .MaximumLength(100)
            .When(p => p.LastName is not null);

        RuleFor(p => p.DateOfBirth)
            .LessThan(DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-18)))
            .When(p => p.DateOfBirth is not null)
            .WithMessage("User must be at least 18 years old.");

        RuleFor(p => p.NationalId)
            .MaximumLength(100)
            .When(p => p.NationalId is not null);

        RuleFor(p => p.Country)
            .Length(2)
            .Matches("^[A-Z]{2}$")
            .When(p => p.Country is not null)
            .WithMessage("Country must be a valid ISO 3166-1 alpha-2 code.");

        RuleFor(p => p.PostalCode)
            .MaximumLength(20)
            .When(p => p.PostalCode is not null);
    }
}
