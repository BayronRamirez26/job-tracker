using FluentValidation;
using JobTracker.Users.Application.Users;
using Microsoft.Extensions.DependencyInjection;

namespace JobTracker.Users.Application;

/// <summary>Registers the Application layer of the Users service.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Consistent English validation messages regardless of the host's OS culture.
        ValidatorOptions.Global.LanguageManager.Enabled = false;

        services.AddValidatorsFromAssemblyContaining<AuthService>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
