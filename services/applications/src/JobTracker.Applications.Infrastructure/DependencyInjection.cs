using JobTracker.Applications.Application.Abstractions;
using JobTracker.Applications.Infrastructure.Persistence;
using JobTracker.Applications.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JobTracker.Applications.Infrastructure;

/// <summary>
/// Registers the Infrastructure layer: the EF Core context on PostgreSQL, and the concrete
/// implementations of the Application layer's ports.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ApplicationsDb")
            ?? throw new InvalidOperationException(
                "Connection string 'ApplicationsDb' was not found. Provide it via configuration, " +
                "user-secrets, or the ConnectionStrings__ApplicationsDb environment variable.");

        services.AddDbContext<ApplicationsDbContext>(options =>
            options
                .UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention());

        services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();

        // The unit of work IS the DbContext — resolve the same scoped instance.
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationsDbContext>());

        return services;
    }
}
