using System.Net;
using System.Text;
using Xunit;
using System.Text.Json;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexusBank.Domain.Enums;
using NexusBank.Infrastructure.Persistence;
using NexusBank.Tests.Integration.Helpers;

namespace NexusBank.Tests.Integration;

public class ClerkWebhookControllerTests(NexusBankWebApplicationFactory factory)
    : IClassFixture<NexusBankWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly IFixture _fixture = new Fixture();

    private NexusDbContext CreateDbContext()
        => factory.Services.CreateScope().ServiceProvider.GetRequiredService<NexusDbContext>();

    private static HttpContent SignedContent(string body)
    {
        var (id, timestamp, signature) = SvixTestHelper.Sign(body);
        var content = new StringContent(body, Encoding.UTF8, "application/json");
        content.Headers.Add("svix-id", id);
        content.Headers.Add("svix-timestamp", timestamp);
        content.Headers.Add("svix-signature", signature);
        return content;
    }

    private string BuildUserCreatedPayload(string clerkUserId, string email, string emailAddressId) =>
        JsonSerializer.Serialize(new
        {
            type = "user.created",
            data = new
            {
                id = clerkUserId,
                email_addresses = new[]
                {
                    new { id = emailAddressId, email_address = email, verification = (object?)null }
                },
                primary_email_address_id = emailAddressId,
                external_accounts = Array.Empty<object>(),
                last_sign_in_at = (long?)null
            }
        });

    private string BuildUserUpdatedPayload(string clerkUserId, string email, string emailAddressId, long? lastSignInAt = null) =>
        JsonSerializer.Serialize(new
        {
            type = "user.updated",
            data = new
            {
                id = clerkUserId,
                email_addresses = new[]
                {
                    new
                    {
                        id = emailAddressId,
                        email_address = email,
                        verification = new
                        {
                            status = "verified",
                            verified_at = DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeMilliseconds()
                        }
                    }
                },
                primary_email_address_id = emailAddressId,
                external_accounts = Array.Empty<object>(),
                last_sign_in_at = lastSignInAt
            }
        });

    private string BuildUserDeletedPayload(string clerkUserId) =>
        JsonSerializer.Serialize(new
        {
            type = "user.deleted",
            data = new
            {
                id = clerkUserId,
                email_addresses = Array.Empty<object>(),
                primary_email_address_id = (string?)null,
                external_accounts = Array.Empty<object>(),
                last_sign_in_at = (long?)null
            }
        });

    // -------------------------------------------------------------------------
    // Signature verification
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Post_WithInvalidSignature_ReturnsUnauthorized()
    {
        // Arrange
        var content = new StringContent("{}", Encoding.UTF8, "application/json");
        content.Headers.Add("svix-id", "msg_fake");
        content.Headers.Add("svix-timestamp", "1234567890");
        content.Headers.Add("svix-signature", "v1,invalidsignature");

        // Act
        var response = await _client.PostAsync("/webhooks/clerk", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            because: "requests with an invalid Svix signature must be rejected before any processing");
    }

    [Fact]
    public async Task Post_WithValidSignature_ReturnsOk()
    {
        // Arrange
        var clerkUserId = $"clerk_{_fixture.Create<string>()}";
        var emailAddressId = _fixture.Create<string>();
        var body = BuildUserCreatedPayload(clerkUserId, $"{_fixture.Create<string>()}@example.com", emailAddressId);

        // Act
        var response = await _client.PostAsync("/webhooks/clerk", SignedContent(body));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            because: "a correctly signed webhook must be accepted");
    }

    // -------------------------------------------------------------------------
    // user.created
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Post_UserCreated_ReturnsOk()
    {
        // Arrange
        var body = BuildUserCreatedPayload(
            $"clerk_{_fixture.Create<string>()}",
            $"{_fixture.Create<string>()}@example.com",
            _fixture.Create<string>());

        // Act
        var response = await _client.PostAsync("/webhooks/clerk", SignedContent(body));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            because: "a valid user.created webhook must be accepted");
    }

    [Fact]
    public async Task Post_UserCreated_PersistsUserWithCorrectFields()
    {
        // Arrange
        var clerkUserId = $"clerk_{_fixture.Create<string>()}";
        var emailAddressId = _fixture.Create<string>();
        var email = $"{_fixture.Create<string>()}@example.com";
        var body = BuildUserCreatedPayload(clerkUserId, email, emailAddressId);

        // Act
        await _client.PostAsync("/webhooks/clerk", SignedContent(body));

        // Assert
        await using var db = CreateDbContext();
        var user = await db.Users.FirstOrDefaultAsync(u => u.ClerkUserId == clerkUserId);

        user.Should().NotBeNull(because: "user.created must persist a new user to the database");
        user!.Email.Should().Be(email, because: "the email must match the primary email address from the payload");
        user.Status.Should().Be(UserStatus.Active, because: "a newly created user must start as active");
        user.KycStatus.Should().Be(KycStatus.Pending, because: "a newly created user must start with pending KYC");
    }

    [Fact]
    public async Task Post_UserCreated_WithGoogleAccount_PersistsIdentity()
    {
        // Arrange
        var clerkUserId = $"clerk_{_fixture.Create<string>()}";
        var emailAddressId = _fixture.Create<string>();
        var body = JsonSerializer.Serialize(new
        {
            type = "user.created",
            data = new
            {
                id = clerkUserId,
                email_addresses = new[]
                {
                    new { id = emailAddressId, email_address = $"{_fixture.Create<string>()}@example.com", verification = (object?)null }
                },
                primary_email_address_id = emailAddressId,
                external_accounts = new[] { new { provider = "google", provider_user_id = "google_999" } },
                last_sign_in_at = (long?)null
            }
        });

        // Act
        await _client.PostAsync("/webhooks/clerk", SignedContent(body));

        // Assert
        await using var db = CreateDbContext();
        var user = await db.Users.Include(u => u.Identities).FirstOrDefaultAsync(u => u.ClerkUserId == clerkUserId);

        user!.Identities.Should().ContainSingle(because: "one Google external account must produce exactly one UserIdentity");
        user.Identities.First().Provider.Should().Be(IdentityProvider.Google,
            because: "the provider string 'google' must map to IdentityProvider.Google");
    }

    [Fact]
    public async Task Post_UserCreated_WhenUserAlreadyExists_DoesNotCreateDuplicate()
    {
        // Arrange
        var clerkUserId = $"clerk_{_fixture.Create<string>()}";
        var emailAddressId = _fixture.Create<string>();
        var body = BuildUserCreatedPayload(clerkUserId, $"{_fixture.Create<string>()}@example.com", emailAddressId);

        // Act — send the same logical event twice with different svix-ids
        await _client.PostAsync("/webhooks/clerk", SignedContent(body));
        await _client.PostAsync("/webhooks/clerk", SignedContent(body));

        // Assert
        await using var db = CreateDbContext();
        var count = await db.Users.CountAsync(u => u.ClerkUserId == clerkUserId);

        count.Should().Be(1, because: "the handler must be idempotent and must not create duplicate users");
    }

    // -------------------------------------------------------------------------
    // user.updated
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Post_UserUpdated_ReturnsOk()
    {
        // Arrange
        var clerkUserId = $"clerk_{_fixture.Create<string>()}";
        var emailAddressId = _fixture.Create<string>();
        await _client.PostAsync("/webhooks/clerk",
            SignedContent(BuildUserCreatedPayload(clerkUserId, $"{_fixture.Create<string>()}@example.com", emailAddressId)));

        var body = BuildUserUpdatedPayload(clerkUserId, $"{_fixture.Create<string>()}@example.com", emailAddressId);

        // Act
        var response = await _client.PostAsync("/webhooks/clerk", SignedContent(body));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            because: "a valid user.updated webhook must be accepted");
    }

    [Fact]
    public async Task Post_UserUpdated_UpdatesEmailAndTimestamps()
    {
        // Arrange
        var clerkUserId = $"clerk_{_fixture.Create<string>()}";
        var emailAddressId = _fixture.Create<string>();
        var updatedEmail = $"{_fixture.Create<string>()}@example.com";
        var lastSignInAt = DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeMilliseconds();

        await _client.PostAsync("/webhooks/clerk",
            SignedContent(BuildUserCreatedPayload(clerkUserId, $"{_fixture.Create<string>()}@example.com", emailAddressId)));

        var body = BuildUserUpdatedPayload(clerkUserId, updatedEmail, emailAddressId, lastSignInAt);

        // Act
        await _client.PostAsync("/webhooks/clerk", SignedContent(body));

        // Assert
        await using var db = CreateDbContext();
        var user = await db.Users.FirstOrDefaultAsync(u => u.ClerkUserId == clerkUserId);

        user!.Email.Should().Be(updatedEmail, because: "user.updated must apply the new email from the payload");
        user.LastSignInAt.Should().NotBeNull(because: "user.updated must apply the last sign-in timestamp from the payload");
        user.EmailVerifiedAt.Should().NotBeNull(because: "user.updated must apply the email verified-at timestamp from the payload");
    }

    // -------------------------------------------------------------------------
    // user.deleted
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Post_UserDeleted_ReturnsOk()
    {
        // Arrange
        var clerkUserId = $"clerk_{_fixture.Create<string>()}";
        var emailAddressId = _fixture.Create<string>();
        await _client.PostAsync("/webhooks/clerk",
            SignedContent(BuildUserCreatedPayload(clerkUserId, $"{_fixture.Create<string>()}@example.com", emailAddressId)));

        var body = BuildUserDeletedPayload(clerkUserId);

        // Act
        var response = await _client.PostAsync("/webhooks/clerk", SignedContent(body));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            because: "a valid user.deleted webhook must be accepted");
    }

    [Fact]
    public async Task Post_UserDeleted_ClosesUserInDatabase()
    {
        // Arrange
        var clerkUserId = $"clerk_{_fixture.Create<string>()}";
        var emailAddressId = _fixture.Create<string>();
        await _client.PostAsync("/webhooks/clerk",
            SignedContent(BuildUserCreatedPayload(clerkUserId, $"{_fixture.Create<string>()}@example.com", emailAddressId)));

        var body = BuildUserDeletedPayload(clerkUserId);

        // Act
        await _client.PostAsync("/webhooks/clerk", SignedContent(body));

        // Assert
        await using var db = CreateDbContext();
        var user = await db.Users.FirstOrDefaultAsync(u => u.ClerkUserId == clerkUserId);

        user!.Status.Should().Be(UserStatus.Closed,
            because: "user.deleted must close the user rather than hard-delete them");
        user.ClosedAt.Should().NotBeNull(because: "ClosedAt must be stamped when the user is closed");
    }

    // -------------------------------------------------------------------------
    // Idempotency (duplicate svix-id)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Post_WithDuplicateSvixId_ReturnsOkOnSecondRequest()
    {
        // Arrange
        var body = BuildUserCreatedPayload(
            $"clerk_{_fixture.Create<string>()}",
            $"{_fixture.Create<string>()}@example.com",
            _fixture.Create<string>());

        var (id, timestamp, signature) = SvixTestHelper.Sign(body);

        HttpContent MakeContent()
        {
            var c = new StringContent(body, Encoding.UTF8, "application/json");
            c.Headers.Add("svix-id", id);
            c.Headers.Add("svix-timestamp", timestamp);
            c.Headers.Add("svix-signature", signature);
            return c;
        }

        await _client.PostAsync("/webhooks/clerk", MakeContent());

        // Act
        var response = await _client.PostAsync("/webhooks/clerk", MakeContent());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            because: "a duplicate svix-id must be acknowledged with 200 rather than rejected");
    }

    [Fact]
    public async Task Post_WithDuplicateSvixId_DoesNotProcessEventTwice()
    {
        // Arrange
        var clerkUserId = $"clerk_{_fixture.Create<string>()}";
        var emailAddressId = _fixture.Create<string>();
        var body = BuildUserCreatedPayload(clerkUserId, $"{_fixture.Create<string>()}@example.com", emailAddressId);

        var (id, timestamp, signature) = SvixTestHelper.Sign(body);

        HttpContent MakeContent()
        {
            var c = new StringContent(body, Encoding.UTF8, "application/json");
            c.Headers.Add("svix-id", id);
            c.Headers.Add("svix-timestamp", timestamp);
            c.Headers.Add("svix-signature", signature);
            return c;
        }

        // Act
        await _client.PostAsync("/webhooks/clerk", MakeContent());
        await _client.PostAsync("/webhooks/clerk", MakeContent());

        // Assert
        await using var db = CreateDbContext();
        var count = await db.Users.CountAsync(u => u.ClerkUserId == clerkUserId);

        count.Should().Be(1, because: "the idempotency cache must prevent the same svix-id from being processed twice");
    }
}
