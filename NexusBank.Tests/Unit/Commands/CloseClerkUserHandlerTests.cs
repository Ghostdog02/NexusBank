using AutoFixture;
using AutoFixture.AutoMoq;
using Xunit;
using FluentAssertions;
using Moq;
using NexusBank.Application.Users.Commands.CloseClerkUser;
using NexusBank.Domain.Entities;
using NexusBank.Domain.Enums;
using NexusBank.Domain.Repositories;

namespace NexusBank.Tests.Unit.Commands;

public class CloseClerkUserHandlerTests
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());

    private (CloseClerkUserHandler handler, Mock<IUserRepository> repo) Setup()
    {
        var repo = _fixture.Freeze<Mock<IUserRepository>>();
        var handler = _fixture.Create<CloseClerkUserHandler>();
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
    public async Task Handle_WhenUserExists_SetsStatusToClosedAndStampsTimestamps()
    {
        // Arrange
        var (handler, repo) = Setup();
        var user = BuildUser();
        repo.Setup(r => r.GetByClerkUserIdAsync(user.ClerkUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        var command = new CloseClerkUserCommand(user.ClerkUserId);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        user.Status.Should().Be(UserStatus.Closed,
            because: "closing a user must set their status to Closed");
        user.ClosedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5),
            because: "the handler must stamp ClosedAt to the current UTC time");
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5),
            because: "the handler must stamp UpdatedAt whenever the user is modified");
        repo.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once,
            "the status change must be persisted");
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_SkipsUpdate()
    {
        // Arrange
        var (handler, repo) = Setup();
        repo.Setup(r => r.GetByClerkUserIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        var command = new CloseClerkUserCommand("clerk_nonexistent");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        repo.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never,
            "the handler must silently no-op when the user is not found");
    }
}
