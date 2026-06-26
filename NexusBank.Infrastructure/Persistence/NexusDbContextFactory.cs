using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NexusBank.Infrastructure.Persistence;

public class NexusDbContextFactory : IDesignTimeDbContextFactory<NexusDbContext>
{
    public NexusDbContext CreateDbContext(string[] args)
    {
        const string secretsPath = "/run/secrets/app_secrets";
        if (File.Exists(secretsPath))
            Env.Load(secretsPath);
        else
            Env.Load();

        var connectionString =
            $"Host={Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost"};" +
            $"Port={Environment.GetEnvironmentVariable("DB_PORT") ?? "5432"};" +
            $"Database={Environment.GetEnvironmentVariable("DB_NAME")};" +
            $"Username={Environment.GetEnvironmentVariable("DB_ADMIN_USER")};" +
            $"Password={Environment.GetEnvironmentVariable("DB_ADMIN_PASSWORD")}";

        var options = new DbContextOptionsBuilder<NexusDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new NexusDbContext(options);
    }
}
