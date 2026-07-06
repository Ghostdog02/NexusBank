namespace NexusBank.Application.Common.Services;

public record SwanOnboardingResult(string OnboardingId, string OnboardingUrl);

public record SwanAccountResult(string Id, string Iban, decimal AvailableBalance, string Status);

public interface ISwanService
{
    Task<SwanOnboardingResult> CreateIndividualOnboardingAsync(string email, string country, CancellationToken ct = default);
    Task SimulateKycApprovalAsync(string onboardingId, CancellationToken ct = default);
    Task<SwanAccountResult?> GetAccountAsync(string accountId, CancellationToken ct = default);
}
