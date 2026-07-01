using NexusBank.Domain.Entities;

namespace NexusBank.Application.Common.Services;

public interface ICurrentUserService
{
    Task<User> GetCurrentUserAsync(CancellationToken ct = default);
}
