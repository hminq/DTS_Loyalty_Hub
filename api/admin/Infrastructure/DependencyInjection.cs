using Infrastructure.Models.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    private static readonly string[] RequiredConfigurationKeys =
    [
        "DATABASE_URL"
    ];

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ValidateRequiredConfiguration(configuration);

        services.AddDbContext<LoyaltyHubDbContext>(options =>
            options.UseNpgsql(configuration["DATABASE_URL"]));

        return services;
    }

    private static void ValidateRequiredConfiguration(IConfiguration configuration)
    {
        var missingKeys = RequiredConfigurationKeys
            .Where(key => string.IsNullOrWhiteSpace(configuration[key]))
            .ToArray();

        if (missingKeys.Length == 0)
        {
            return;
        }

        throw new InvalidOperationException(
            $"Missing required configuration value(s): {string.Join(", ", missingKeys)}");
    }
}
