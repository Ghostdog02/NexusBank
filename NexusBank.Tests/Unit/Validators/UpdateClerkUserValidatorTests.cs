using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;
using NexusBank.Application.Users.Commands.UpdateClerkUser;

namespace NexusBank.Tests.Unit.Validators;

public class UpdateClerkUserValidatorTests
{
    private readonly UpdateClerkUserValidator _validator = new();

    [Fact]
    public void Validate_WhenCommandIsValid_Passes()
    {
        // Arrange
        var command = new UpdateClerkUserCommand("clerk_123", "user@example.com", null, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue(because: "all required fields are present and well-formed");
    }

    [Fact]
    public void Validate_WhenCommandIsValid_WithAllFields_Passes()
    {
        // Arrange
        var command = new UpdateClerkUserCommand(
            "clerk_123",
            "user@example.com",
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddHours(-1));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue(because: "all fields are present and within valid ranges");
    }

    [Fact]
    public void Validate_WhenClerkUserIdIsEmpty_Fails()
    {
        // Arrange
        var command = new UpdateClerkUserCommand("", "user@example.com", null, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.ClerkUserId)
            .WithErrorMessage("'Clerk User Id' must not be empty.");
    }

    [Fact]
    public void Validate_WhenEmailIsEmpty_Fails()
    {
        // Arrange
        var command = new UpdateClerkUserCommand("clerk_123", "", null, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Email);
    }

    [Fact]
    public void Validate_WhenEmailIsInvalidFormat_Fails()
    {
        // Arrange
        var command = new UpdateClerkUserCommand("clerk_123", "not-an-email", null, null);

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
        var command = new UpdateClerkUserCommand("clerk_123", email, null, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Email);
    }

    [Fact]
    public void Validate_WhenLastSignInAtIsInTheFuture_Fails()
    {
        // Arrange
        var command = new UpdateClerkUserCommand(
            "clerk_123",
            "user@example.com",
            null,
            DateTime.UtcNow.AddMinutes(5));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.LastSignInAt);
    }

    [Fact]
    public void Validate_WhenLastSignInAtIsNull_SkipsLastSignInAtValidation()
    {
        // Arrange
        var command = new UpdateClerkUserCommand("clerk_123", "user@example.com", null, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.LastSignInAt);
    }
}
