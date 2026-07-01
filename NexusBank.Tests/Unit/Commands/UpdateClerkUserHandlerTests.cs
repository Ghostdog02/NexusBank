using AutoFixture;
using AutoFixture.AutoMoq;
using Xunit;
using FluentAssertions;
using Moq;
using NexusBank.Application.Users.Commands.UpdateClerkUser;
using NexusBank.Domain.Entities;
using NexusBank.Domain.Repositories;

namespace NexusBank.Tests.Unit.Commands;

public class UpdateClerkUserHandlerTests
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());

    private (UpdateClerkUserHandler handler, Mock<IUserRepository> repo) Setup()
    {
        var repo = _fixture.Freeze<Mock<IUserRepository>>();
        var handler = _fixture.Create<UpdateClerkUserHandler>();
        return (handler, repo);
    }

    private User BuildUser() =>
        // Profile is nullable — omit to avoid AutoFixture failing on required UserProfile fields.
        // Identities is set to empty so the fixture doesn't recurse into UserIdentity navigation properties.
        _fixture.Build<User>()
            .Without(u => u.Profile)
            .With(u => u.Identities, [])
            .Create();

    [Fact]
    public async Task Handle_WhenUserExists_UpdatesEmailAndTimestamps()
    {
        // Arrange
        var (handler, repo) = Setup();
        var user = BuildUser();
        var verifiedAt = DateTime.UtcNow.AddDays(-1);
        var lastSignInAt = DateTime.UtcNow.AddHours(-1);
        repo.Setup(r => r.GetByClerkUserIdAsync(user.ClerkUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        var command = new UpdateClerkUserCommand(user.ClerkUserId, "new@example.com", verifiedAt, lastSignInAt);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        user.Email.Should().Be("new@example.com", "the handler must apply the email from the command");
        user.EmailVerifiedAt.Should().Be(verifiedAt, "the handler must apply the verification timestamp from the command");
        user.LastSignInAt.Should().Be(lastSignInAt, "the handler must apply the last sign-in timestamp from the command");
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5), "the handler must stamp UpdatedAt to now");
        repo.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once,
            "the handler must persist the changes");
    }

    [Fact]
    public async Task Handle_WhenUserExists_AndNullableFieldsAreNull_ClearsThemOnUser()
    {
        // Arrange
        var (handler, repo) = Setup();
        var user = BuildUser();
        repo.Setup(r => r.GetByClerkUserIdAsync(user.ClerkUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        var command = new UpdateClerkUserCommand(user.ClerkUserId, "new@example.com", null, null);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        user.EmailVerifiedAt.Should().BeNull("a null command value must clear the field on the user");
        user.LastSignInAt.Should().BeNull("a null command value must clear the field on the user");
        repo.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once,
            "update must still be persisted even when nullable fields are null");
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_SkipsUpdate()
    {
        // Arrange
        var (handler, repo) = Setup();
        repo.Setup(r => r.GetByClerkUserIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        var command = new UpdateClerkUserCommand("clerk_nonexistent", "new@example.com", null, null);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        repo.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never,
            "the handler must silently no-op when the user is not found");
    }
}
