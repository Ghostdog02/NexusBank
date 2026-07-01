using FluentValidation;

namespace NexusBank.Application.Users.Commands.CreateClerkUser;

public class CreateClerkUserValidator : AbstractValidator<CreateClerkUserCommand>
{
    public CreateClerkUserValidator()
    {
        RuleFor(c => c.ClerkUserId)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(c => c.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255);

        When(c => c.ProviderUserId is not null, () =>
        {
            RuleFor(c => c.ProviderUserId)
                .MaximumLength(255);
        });
    }
}
