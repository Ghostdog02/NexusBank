using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;
using NexusBank.Application.Users.Commands.CreateClerkUser;
using NexusBank.Domain.Enums;

namespace NexusBank.Tests.Unit.Validators;

public class CreateClerkUserValidatorTests
{
    private readonly CreateClerkUserValidator _validator = new();

    [Fact]
    public void Validate_WhenCommandIsValid_Passes()
    {
        // Arrange
        var command = new CreateClerkUserCommand("clerk_123", "user@example.com", null, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue(because: "all required fields are present and well-formed");
    }

    [Fact]
    public void Validate_WhenCommandIsValid_WithProvider_Passes()
    {
        // Arrange
        var command = new CreateClerkUserCommand("clerk_123", "user@example.com", IdentityProvider.Google, "google_456");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue(because: "provider and providerUserId are both optional and valid when supplied together");
    }

    [Fact]
    public void Validate_WhenClerkUserIdIsEmpty_Fails()
    {
        // Arrange
        var command = new CreateClerkUserCommand("", "user@example.com", null, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.ClerkUserId)
            .WithErrorMessage("'Clerk User Id' must not be empty.");
    }

    [Fact]
    public void Validate_WhenClerkUserIdExceedsMaxLength_Fails()
    {
        // Arrange
        var command = new CreateClerkUserCommand(new string('a', 256), "user@example.com", null, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.ClerkUserId);
    }

    [Fact]
    public void Validate_WhenEmailIsEmpty_Fails()
    {
        // Arrange
        var command = new CreateClerkUserCommand("clerk_123", "", null, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Email);
    }

    [Fact]
    public void Validate_WhenEmailIsInvalidFormat_Fails()
    {
        // Arrange
        var command = new CreateClerkUserCommand("clerk_123", "not-an-email", null, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Email)
            .WithErrorMessage("'Email' is not a valid email address.");
    }

    [Fact]
    public void Validate_WhenEmailExceedsMaxLength_Fails()
    {
        // Arrange
        var email = $"{new string('a', 244)}@example.com";
        var command = new CreateClerkUserCommand("clerk_123", email, null, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Email);
    }

    [Fact]
    public void Validate_WhenProviderUserIdExceedsMaxLength_Fails()
    {
        // Arrange
        var command = new CreateClerkUserCommand("clerk_123", "user@example.com", IdentityProvider.Google, new string('a', 256));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.ProviderUserId);
    }

    [Fact]
    public void Validate_WhenProviderUserIdIsNull_SkipsProviderUserIdValidation()
    {
        // Arrange
        var command = new CreateClerkUserCommand("clerk_123", "user@example.com", null, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.ProviderUserId);
    }
}
