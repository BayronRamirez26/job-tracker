using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace JobTracker.Applications.Infrastructure.Persistence;

/// <summary>
/// Lets the <c>dotnet ef</c> tooling build the <see cref="ApplicationsDbContext"/> at design time
/// (to add or apply migrations) without booting the API host.
/// </summary>
/// <remarks>
/// <c>migrations add</c> only reads the model, so the connection string is irrelevant there;
/// <c>database update</c> uses it to connect. Override it with the
/// <c>ConnectionStrings__ApplicationsDb</c> environment variable when applying migrations against
/// a specific database. The naming convention MUST match the runtime configuration in
/// <c>AddInfrastructure</c>, or the generated model would disagree with the running app.
/// </remarks>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationsDbContext>
{
    public ApplicationsDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__ApplicationsDb")
            ?? "Host=localhost;Port=5432;Database=applications;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<ApplicationsDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new ApplicationsDbContext(options);
    }
}
