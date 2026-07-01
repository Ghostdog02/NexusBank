using FluentValidation;
using NexusBank.Domain.Entities;

namespace NexusBank.Application.Common.Validators;

public class UserProfileValidator : AbstractValidator<UserProfile>
{
    public UserProfileValidator()
    {
        RuleFor(p => p.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(p => p.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(p => p.DateOfBirth)
            .LessThan(DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-18)))
            .WithMessage("User must be at least 18 years old.");

        RuleFor(p => p.NationalId)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(p => p.Country)
            .NotEmpty()
            .Length(2)
            .Matches("^[A-Z]{2}$")
            .WithMessage("Country must be a valid ISO 3166-1 alpha-2 code.");

        RuleFor(p => p.PostalCode)
            .MaximumLength(20)
            .When(p => p.PostalCode is not null);
    }
}
