using Core.Abstractions;
using Infrastructure.Auth;
using Infrastructure.Implementations;
using Infrastructure.Models.Context;
using Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var databaseOptions = DatabaseOptions.FromConfiguration(configuration);
        var jwtOptions = JwtOptions.FromConfiguration(configuration);

        services.AddDbContext<LoyaltyHubDbContext>(options =>
            options.UseNpgsql(databaseOptions.ConnectionString));

        services.AddSingleton(databaseOptions);
        services.AddSingleton(jwtOptions);
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAdminSessionRepository, AdminSessionRepository>();
        services.AddScoped<IAdminPermissionChecker, AdminPermissionChecker>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPasswordVerifier, PasswordVerifier>();
        services.AddScoped<IAccessTokenService, JwtAccessTokenService>();
        return services;
    }
}
