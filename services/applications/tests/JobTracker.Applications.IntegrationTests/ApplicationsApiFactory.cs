using System.Security.Claims;
using System.Text.Encodings.Web;
using JobTracker.Applications.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Testcontainers.PostgreSql;

namespace JobTracker.Applications.IntegrationTests;

/// <summary>
/// Boots the real API in memory (WebApplicationFactory) against a throwaway PostgreSQL container.
/// Authentication is swapped for a test scheme that signs every request in as a fixed user, so the
/// [Authorize] endpoints are exercised without minting real JWTs; the JWT config is still supplied
/// so the app's startup validation passes.
/// </summary>
public sealed class ApplicationsApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string ConnectionStringEnvVar = "ConnectionStrings__ApplicationsDb";
    public const string TestScheme = "Test";
    public static readonly Guid TestUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .WithDatabase("applications_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Replace JWT bearer with a test scheme that authenticates every request as TestUserId.
            services.AddAuthentication(TestScheme)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestScheme, _ => { });
        });
    }

    public async Task InitializeAsync()
    {
        await _database.StartAsync();

        // Set BEFORE the host is first built (the Services access below).
        Environment.SetEnvironmentVariable(ConnectionStringEnvVar, _database.GetConnectionString());
        Environment.SetEnvironmentVariable("Jwt__SigningKey", "test-signing-key-0123456789abcdef0123456789abcdef");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "jobtracker");
        Environment.SetEnvironmentVariable("Jwt__Audience", "jobtracker");

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationsDbContext>();
        await context.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        Environment.SetEnvironmentVariable(ConnectionStringEnvVar, null);
        Environment.SetEnvironmentVariable("Jwt__SigningKey", null);
        Environment.SetEnvironmentVariable("Jwt__Issuer", null);
        Environment.SetEnvironmentVariable("Jwt__Audience", null);
        await _database.DisposeAsync();
        await base.DisposeAsync();
    }

    /// <summary>Clears all rows so each test starts from a known-empty table.</summary>
    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationsDbContext>();
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE job_applications;");
    }

    private sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[] { new Claim("sub", TestUserId.ToString()) };
            var identity = new ClaimsIdentity(claims, TestScheme);
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), TestScheme);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
