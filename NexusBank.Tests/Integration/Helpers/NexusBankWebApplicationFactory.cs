using DotNetEnv;
using NexusBank.Api;
using Xunit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using NexusBank.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace NexusBank.Tests.Integration.Helpers;

public class NexusBankWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithDatabase("nexusbank_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        // Make the webhook secret available to the app before it boots.
        Environment.SetEnvironmentVariable("CLERK_WEBHOOK_SECRET", SvixTestHelper.TestSecret);

        // Run EF migrations against the test container so the schema is ready.
        var options = new DbContextOptionsBuilder<NexusDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .UseSnakeCaseNamingConvention()
            .Options;

        await using var db = new NexusDbContext(options);
        await db.Database.MigrateAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Use a dedicated test environment so Program.cs dev-only blocks (auto-migration, OpenAPI) don't run.
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Replace the production DbContext registration with one pointing at the test container.
            var dbDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<NexusDbContext>));
            if (dbDescriptor != null) services.Remove(dbDescriptor);

            services.AddDbContext<NexusDbContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString())
                       .UseSnakeCaseNamingConvention());

            // Replace Redis with an in-memory distributed cache — no Redis container needed in tests.
            var cacheDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IDistributedCache));
            if (cacheDescriptor != null) services.Remove(cacheDescriptor);

            services.AddDistributedMemoryCache();
        });
    }

    public new async Task DisposeAsync()
    {
        await _postgres.StopAsync();
        await base.DisposeAsync();
    }
}
