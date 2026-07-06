using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;

namespace NexusBank.Infrastructure.External.Swan;

public class SwanTokenProvider(HttpClient httpClient, SwanOptions options, IMemoryCache cache)
{
    private const string CacheKey = "swan:access-token";
    private static readonly TimeSpan RefreshMargin = TimeSpan.FromMinutes(5);

    public Task<string> GetAccessTokenAsync(CancellationToken ct = default)
        => cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            var (token, expiresInSeconds) = await FetchTokenAsync(ct);

            var ttl = TimeSpan.FromSeconds(expiresInSeconds) - RefreshMargin;
            entry.AbsoluteExpirationRelativeToNow = ttl > TimeSpan.Zero ? ttl : TimeSpan.FromSeconds(expiresInSeconds);

            return token;
        })!;

    private async Task<(string Token, int ExpiresIn)> FetchTokenAsync(CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, options.OAuthUrl)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = options.ClientId,
                ["client_secret"] = options.ClientSecret,
            })
        };

        using var response = await httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Swan token endpoint returned an empty response.");

        return (payload.AccessToken, payload.ExpiresIn);
    }

    private record TokenResponse(
        [property: System.Text.Json.Serialization.JsonPropertyName("access_token")] string AccessToken,
        [property: System.Text.Json.Serialization.JsonPropertyName("expires_in")] int ExpiresIn);
}
