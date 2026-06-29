using DotNetEnv;
using Microsoft.AspNetCore.DataProtection;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using MediatR;
using NexusBank.Application.Common.Behaviours;
using NexusBank.Application.Common.Validators;
using NexusBank.Application.Users.Commands.CreateClerkUser;
using NexusBank.Domain.Repositories;
using NexusBank.Infrastructure.Persistence;
using NexusBank.Infrastructure.Persistence.Repositories;

namespace NexusBank.Api;

static class Program
{
    public static async Task Main(string[] args)
    {
        var secretsPath = "/run/secrets/app_secrets";
        if (File.Exists(secretsPath))
            Env.Load(secretsPath);
        else
            Env.Load();

        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo("/root/.aspnet/DataProtection-Keys"));

        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = builder.Configuration["Clerk:Authority"];
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["Clerk:Authority"],
                    ValidateAudience = false,
                    ValidateLifetime = true,
                };
            });

        builder.Services.AddAuthorization();

        var appConnectionString =
            $"Host={Environment.GetEnvironmentVariable("DB_HOST")};" +
            $"Port={Environment.GetEnvironmentVariable("DB_PORT")};" +
            $"Database={Environment.GetEnvironmentVariable("DB_NAME")};" +
            $"Username={Environment.GetEnvironmentVariable("DB_APP_USER")};" +
            $"Password={Environment.GetEnvironmentVariable("DB_APP_PASSWORD")}";

        builder.Services.AddDbContext<NexusDbContext>(options =>
            options.UseNpgsql(appConnectionString));

        builder.Services.AddHealthChecks()
            .AddNpgSql(
                appConnectionString,
                name: "database",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready"]);

        builder.Services.AddValidatorsFromAssemblyContaining<UserValidator>();

        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CreateClerkUserCommand).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        });

        builder.Services.AddScoped<IUserRepository, UserRepository>();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<NexusDbContext>();
            await db.Database.MigrateAsync();

            app.MapOpenApi();
        }

        app.MapHealthChecks("/health");

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        });

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
