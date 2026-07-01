using System.Security.Cryptography;
using System.Text;
using DotNetEnv;

namespace NexusBank.Tests.Integration.Helpers;

public static class SvixTestHelper
{
    // Loaded once from .env.test so the secret is not hardcoded in source.
    public static readonly string TestSecret;

    static SvixTestHelper()
    {
        Env.Load(Path.Combine(GetRepoRoot(), ".env.test"));
        TestSecret = Environment.GetEnvironmentVariable("CLERK_WEBHOOK_SECRET")
            ?? throw new InvalidOperationException("CLERK_WEBHOOK_SECRET not found in .env.test");
    }

    /// <summary>
    /// Generates valid Svix webhook headers for the given body so the controller's signature check passes.
    /// Svix signs: "{svix-id}.{svix-timestamp}.{body}" with HMAC-SHA256 using the decoded secret bytes.
    /// </summary>
    public static (string SvixId, string SvixTimestamp, string SvixSignature) Sign(string body)
    {
        var id = $"msg_{Guid.NewGuid():N}";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

        var secretBytes = Convert.FromBase64String(TestSecret["whsec_".Length..]);
        var toSign = Encoding.UTF8.GetBytes($"{id}.{timestamp}.{body}");

        using var hmac = new HMACSHA256(secretBytes);
        var signature = $"v1,{Convert.ToBase64String(hmac.ComputeHash(toSign))}";

        return (id, timestamp, signature);
    }

    private static string GetRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "NexusBank.slnx")))
            dir = dir.Parent;
        return dir?.FullName ?? throw new InvalidOperationException("Could not locate repo root from test output directory");
    }
}
