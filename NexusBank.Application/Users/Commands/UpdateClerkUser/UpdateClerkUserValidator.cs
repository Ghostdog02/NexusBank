using FluentValidation;

namespace NexusBank.Application.Users.Commands.UpdateClerkUser;

public class UpdateClerkUserValidator : AbstractValidator<UpdateClerkUserCommand>
{
    public UpdateClerkUserValidator()
    {
        RuleFor(c => c.ClerkUserId)
            .NotEmpty();

        RuleFor(c => c.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255);

        When(c => c.LastSignInAt is not null, () =>
        {
            RuleFor(c => c.LastSignInAt)
                .LessThanOrEqualTo(_ => DateTime.UtcNow);
        });
    }
}
