using DotNetEnv;
using Microsoft.AspNetCore.DataProtection;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using NexusBank.Application.Common.Behaviours;
using NexusBank.Application.Common.Validators;
using NexusBank.Application.Users.Commands.CreateClerkUser;
using NexusBank.Domain.Repositories;
using NexusBank.Infrastructure.Persistence;
using NexusBank.Infrastructure.Persistence.Repositories;
using NexusBank.Api.Services;
using NexusBank.Api.Authorization;
using NexusBank.Api.Middleware;
using NexusBank.Application.Common.Authorization;
using NexusBank.Application.Common.Services;
using NexusBank.Domain.Enums;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;

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

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog((context, services, config) => config
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console());

        builder.Services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo("/root/.aspnet/DataProtection-Keys"));

        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");
        });
        builder.Services.AddProblemDetails();
        builder.Services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("webhook", limiter =>
            {
                limiter.PermitLimit = 300;
                limiter.Window = TimeSpan.FromMinutes(1);
                limiter.QueueLimit = 0;
            });
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });
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

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("AuthenticatedUser", policy =>
                policy.RequireAuthenticatedUser());

            options.AddPolicy("AdminOnly", policy =>
                policy.AddRequirements(new RoleRequirement(UserRole.Admin)));
        });

        var appConnectionString =
            $"Host={Environment.GetEnvironmentVariable("DB_HOST")};" +
            $"Port={Environment.GetEnvironmentVariable("DB_PORT")};" +
            $"Database={Environment.GetEnvironmentVariable("DB_NAME")};" +
            $"Username={Environment.GetEnvironmentVariable("DB_APP_USER")};" +
            $"Password={Environment.GetEnvironmentVariable("DB_APP_PASSWORD")}";

        builder.Services.AddDbContext<NexusDbContext>(options =>
            options.UseNpgsql(appConnectionString).UseSnakeCaseNamingConvention());

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

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
        builder.Services.AddScoped<IAuthorizationHandler, RoleRequirementHandler>();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            var adminConnectionString =
                $"Host={Environment.GetEnvironmentVariable("DB_HOST")};" +
                $"Port={Environment.GetEnvironmentVariable("DB_PORT")};" +
                $"Database={Environment.GetEnvironmentVariable("DB_NAME")};" +
                $"Username={Environment.GetEnvironmentVariable("DB_ADMIN_USER")};" +
                $"Password={Environment.GetEnvironmentVariable("DB_ADMIN_PASSWORD")}";

            var migrationOptions = new DbContextOptionsBuilder<NexusDbContext>()
                .UseNpgsql(adminConnectionString)
                .UseSnakeCaseNamingConvention()
                .Options;

            await using var migrationDb = new NexusDbContext(migrationOptions);
            await migrationDb.Database.MigrateAsync();

            app.MapOpenApi();
        }

        app.MapHealthChecks("/health");

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        });

        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseSerilogRequestLogging();
        app.UseRateLimiter();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        await app.RunAsync();
    }
}
