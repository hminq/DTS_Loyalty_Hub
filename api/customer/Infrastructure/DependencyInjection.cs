using Core.Abstractions;
using Infrastructure.Implementations;
using Infrastructure.Auth;
using Persistence.Models.Context;
using Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Behaviors;
using MediatR;

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
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IPointTransactionRepository, PointTransactionRepository>();
        services.AddScoped<ICustomerVoucherRepository, CustomerVoucherRepository>();
        services.AddScoped<IPasswordVerifier, PasswordVerifier>();
        services.AddScoped<IAccessTokenService, JwtAccessTokenService>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(SaveChangesBehavior<,>));

        return services;
    }
}
