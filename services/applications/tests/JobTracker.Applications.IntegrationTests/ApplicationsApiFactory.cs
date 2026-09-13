using JobTracker.Applications.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace JobTracker.Applications.IntegrationTests;

/// <summary>
/// Boots the real API in memory (WebApplicationFactory) against a throwaway PostgreSQL container
/// started by Testcontainers. This gives genuine end-to-end coverage — real HTTP, real Npgsql,
/// real migrations — instead of an in-memory fake that wouldn't catch provider-specific issues.
/// </summary>
public sealed class ApplicationsApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string ConnectionStringEnvVar = "ConnectionStrings__ApplicationsDb";

    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .WithDatabase("applications_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public async Task InitializeAsync()
    {
        await _database.StartAsync();

        // The API reads ConnectionStrings:ApplicationsDb from configuration (env vars included).
        // Setting it BEFORE the host is first built (the Services access below) makes the real
        // AddInfrastructure point the DbContext at this container — no test-only DI overrides,
        // and it works on CI where there are no user-secrets.
        Environment.SetEnvironmentVariable(ConnectionStringEnvVar, _database.GetConnectionString());

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationsDbContext>();
        await context.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        Environment.SetEnvironmentVariable(ConnectionStringEnvVar, null);
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
}
