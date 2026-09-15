using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace JobTracker.Users.Infrastructure.Persistence;

/// <summary>
/// Lets `dotnet ef` build the context for migrations without booting the API. The Users database
/// lives on port 5433 (the Applications DB is on 5432) — one database per service. Override with
/// the ConnectionStrings__UsersDb environment variable when applying migrations elsewhere.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<UsersDbContext>
{
    public UsersDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__UsersDb")
            ?? "Host=localhost;Port=5433;Database=users;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<UsersDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new UsersDbContext(options);
    }
}
