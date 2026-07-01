using AutoFixture;
using AutoFixture.AutoMoq;
using Xunit;
using FluentAssertions;
using Moq;
using NexusBank.Application.Users.Commands.CreateClerkUser;
using NexusBank.Domain.Entities;
using NexusBank.Domain.Enums;
using NexusBank.Domain.Repositories;

namespace NexusBank.Tests.Unit.Commands;

public class CreateClerkUserHandlerTests
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());

    private (CreateClerkUserHandler handler, Mock<IUserRepository> repo) Setup()
    {
        var repo = _fixture.Freeze<Mock<IUserRepository>>();
        var handler = _fixture.Create<CreateClerkUserHandler>();
        return (handler, repo);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_AddsNewUser()
    {
        // Arrange
        var (handler, repo) = Setup();
        repo.Setup(r => r.GetByClerkUserIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        var command = new CreateClerkUserCommand("clerk_123", "user@example.com", null, null);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        repo.Verify(r => r.AddAsync(
            It.Is<User>(u =>
                u.ClerkUserId == "clerk_123" &&
                u.Email == "user@example.com" &&
                u.Status == UserStatus.Active &&
                u.KycStatus == KycStatus.Pending &&
                u.Identities.Count == 0),
            It.IsAny<CancellationToken>()), Times.Once,
            "a new user must be persisted when no existing user matches the ClerkUserId");
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_AndProviderProvided_AddsUserWithIdentity()
    {
        // Arrange
        var (handler, repo) = Setup();
        repo.Setup(r => r.GetByClerkUserIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        var command = new CreateClerkUserCommand("clerk_123", "user@example.com", IdentityProvider.Google, "google_456");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        repo.Verify(r => r.AddAsync(
            It.Is<User>(u =>
                u.Identities.Count == 1 &&
                u.Identities.First().Provider == IdentityProvider.Google &&
                u.Identities.First().ProviderUserId == "google_456"),
            It.IsAny<CancellationToken>()), Times.Once,
            "a UserIdentity must be created and linked when both provider and providerUserId are supplied");
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyExists_SkipsAdd()
    {
        // Arrange
        var (handler, repo) = Setup();
        // Profile is nullable — omit to avoid AutoFixture failing on required UserProfile fields.
        // Identities were set to empty so the fixture doesn't recurse into UserIdentity navigation properties.
        var existingUser = _fixture.Build<User>()
            .Without(u => u.Profile)
            .With(u => u.Identities, [])
            .Create();
        repo.Setup(r => r.GetByClerkUserIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);
        var command = new CreateClerkUserCommand("clerk_123", "user@example.com", null, null);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        repo.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never,
            "the handler must be idempotent and skip creation when the user already exists");
    }

    [Fact]
    public async Task Handle_WhenProviderIsNullButProviderUserIdIsSet_DoesNotAddIdentity()
    {
        // Arrange
        var (handler, repo) = Setup();
        repo.Setup(r => r.GetByClerkUserIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        var command = new CreateClerkUserCommand("clerk_123", "user@example.com", null, "google_456");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        repo.Verify(r => r.AddAsync(
            It.Is<User>(u => u.Identities.Count == 0),
            It.IsAny<CancellationToken>()), Times.Once,
            "both provider and providerUserId must be non-null to create a UserIdentity");
    }
}
