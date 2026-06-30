using System.Net;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NexusBank.Api.Webhooks;
using NexusBank.Application.Users.Commands.CloseClerkUser;
using NexusBank.Application.Users.Commands.CreateClerkUser;
using NexusBank.Application.Users.Commands.UpdateClerkUser;
using NexusBank.Domain.Enums;
using Svix;

namespace NexusBank.Api.Controllers;

[ApiController]
[Route("webhooks/clerk")]
public class ClerkWebhookController(IMediator mediator) : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [HttpPost]
    [EnableRateLimiting("webhook")]
    public async Task<IActionResult> Handle(
        [FromHeader(Name = "svix-id")] string svixId,
        [FromHeader(Name = "svix-timestamp")] string svixTimestamp,
        [FromHeader(Name = "svix-signature")] string svixSignature,
        CancellationToken ct)
    {
        Request.EnableBuffering();
        var body = await new StreamReader(Request.Body).ReadToEndAsync(ct);

        var secret = System.Environment.GetEnvironmentVariable("CLERK_WEBHOOK_SECRET")!;
        var wh = new Webhook(secret);

        try
        {
            var headers = new WebHeaderCollection
            {
                { "svix-id", svixId },
                { "svix-timestamp", svixTimestamp },
                { "svix-signature", svixSignature }
            };
            wh.Verify(body, headers);
        }
        catch
        {
            return Unauthorized();
        }

        var payload = JsonSerializer.Deserialize<ClerkEventPayload>(body, JsonOptions);
        if (payload is null) return BadRequest();

        switch (payload.Type)
        {
            case "user.created":
                await mediator.Send(BuildCreateCommand(payload.Data), ct);
                break;
            case "user.updated":
                await mediator.Send(BuildUpdateCommand(payload.Data), ct);
                break;
            case "user.deleted":
                await mediator.Send(new CloseClerkUserCommand(payload.Data.Id), ct);
                break;
        }

        return Ok();
    }

    private static CreateClerkUserCommand BuildCreateCommand(ClerkUserData data)
    {
        var primaryEmail = data.EmailAddresses
            .First(e => e.Id == data.PrimaryEmailAddressId);

        var externalAccount = data.ExternalAccounts.FirstOrDefault();

        return new CreateClerkUserCommand(
            ClerkUserId: data.Id,
            Email: primaryEmail.EmailAddress,
            Provider: externalAccount is not null ? MapProvider(externalAccount.Provider) : null,
            ProviderUserId: externalAccount?.ProviderUserId
        );
    }

    private static UpdateClerkUserCommand BuildUpdateCommand(ClerkUserData data)
    {
        var primaryEmail = data.EmailAddresses
            .First(e => e.Id == data.PrimaryEmailAddressId);

        var verifiedAt = primaryEmail.Verification is { Status: "verified", VerifiedAt: not null }
            ? DateTimeOffset.FromUnixTimeMilliseconds(primaryEmail.Verification.VerifiedAt.Value).UtcDateTime
            : (DateTime?)null;

        var lastSignInAt = data.LastSignInAt is not null
            ? DateTimeOffset.FromUnixTimeMilliseconds(data.LastSignInAt.Value).UtcDateTime
            : (DateTime?)null;

        return new UpdateClerkUserCommand(
            ClerkUserId: data.Id,
            Email: primaryEmail.EmailAddress,
            EmailVerifiedAt: verifiedAt,
            LastSignInAt: lastSignInAt
        );
    }

    private static IdentityProvider MapProvider(string provider) => provider.ToLowerInvariant() switch
    {
        "google" => IdentityProvider.Google,
        "github" => IdentityProvider.GitHub,
        "apple" => IdentityProvider.Apple,
        _ => IdentityProvider.Email
    };
}
