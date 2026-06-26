using System.Text.Json.Serialization;

namespace NexusBank.Api.Webhooks;

public record ClerkEventPayload(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("data")] ClerkUserData Data
);

public record ClerkUserData(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("email_addresses")] List<ClerkEmailAddress> EmailAddresses,
    [property: JsonPropertyName("primary_email_address_id")] string PrimaryEmailAddressId,
    [property: JsonPropertyName("external_accounts")] List<ClerkExternalAccount> ExternalAccounts,
    [property: JsonPropertyName("last_sign_in_at")] long? LastSignInAt
);

public record ClerkEmailAddress(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("email_address")] string EmailAddress,
    [property: JsonPropertyName("verification")] ClerkVerification? Verification
);

public record ClerkVerification(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("expire_at")] long? VerifiedAt
);

public record ClerkExternalAccount(
    [property: JsonPropertyName("provider")] string Provider,
    [property: JsonPropertyName("provider_user_id")] string ProviderUserId
);
