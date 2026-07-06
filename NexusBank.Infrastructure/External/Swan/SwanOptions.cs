namespace NexusBank.Infrastructure.External.Swan;

public class SwanOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string OAuthUrl { get; set; } = string.Empty;
    public string ApiUrl { get; set; } = string.Empty;
    public string OnboardingRedirectUrl { get; set; } = string.Empty;
}
