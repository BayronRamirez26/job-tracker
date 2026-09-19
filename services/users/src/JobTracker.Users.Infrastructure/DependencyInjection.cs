using JobTracker.Users.Application.Abstractions;
using JobTracker.Users.Infrastructure.Persistence;
using JobTracker.Users.Infrastructure.Persistence.Repositories;
using JobTracker.Users.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JobTracker.Users.Infrastructure;

/// <summary>Registers the Infrastructure layer: EF Core on PostgreSQL, the repository, and the security services.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("UsersDb")
            ?? throw new InvalidOperationException(
                "Connection string 'UsersDb' was not found. Provide it via configuration, " +
                "user-secrets, or the ConnectionStrings__UsersDb environment variable.");

        services.AddDbContext<UsersDbContext>(options =>
            options
                .UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention());

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProfileRepository, ProfileRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<UsersDbContext>());

        // JWT settings + stateless security services.
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenGenerator, JwtTokenGenerator>();

        return services;
    }
}
