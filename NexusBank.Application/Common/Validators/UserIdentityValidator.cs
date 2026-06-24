using FluentValidation;
using NexusBank.Domain.Entities;

namespace NexusBank.Application.Common.Validators;

public class UserIdentityValidator : AbstractValidator<UserIdentity>
{
    public UserIdentityValidator()
    {
        RuleFor(i => i.Provider)
            .IsInEnum();

        RuleFor(i => i.ProviderUserId)
            .NotEmpty()
            .MaximumLength(255);
    }
}
