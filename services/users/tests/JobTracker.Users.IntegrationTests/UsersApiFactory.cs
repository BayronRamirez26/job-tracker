using JobTracker.Users.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace JobTracker.Users.IntegrationTests;

/// <summary>
/// Boots the real Users API against a throwaway PostgreSQL container. Both the connection string
/// and the JWT signing key are supplied via environment variables set before the host is built,
/// so the real AddInfrastructure / JWT setup runs unchanged and this works on CI (no user-secrets).
/// Env vars have higher precedence than user-secrets, so they win even in Development.
/// </summary>
public sealed class UsersApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string ConnectionStringEnvVar = "ConnectionStrings__UsersDb";
    private const string SigningKeyEnvVar = "Jwt__SigningKey";

    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .WithDatabase("users_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public async Task InitializeAsync()
    {
        await _database.StartAsync();

        Environment.SetEnvironmentVariable(ConnectionStringEnvVar, _database.GetConnectionString());
        Environment.SetEnvironmentVariable(SigningKeyEnvVar, "integration-test-signing-key-0123456789abcdef0123456789");

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        await context.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        Environment.SetEnvironmentVariable(ConnectionStringEnvVar, null);
        Environment.SetEnvironmentVariable(SigningKeyEnvVar, null);
        await _database.DisposeAsync();
        await base.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE users;");
    }
}
