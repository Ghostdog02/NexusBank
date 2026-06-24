using FluentValidation;
using NexusBank.Domain.Entities;

namespace NexusBank.Application.Common.Validators;

public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(u => u.ClerkUserId)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(u => u.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255);

        RuleFor(u => u.Status)
            .IsInEnum();

        RuleFor(u => u.KycStatus)
            .IsInEnum();
    }
}
