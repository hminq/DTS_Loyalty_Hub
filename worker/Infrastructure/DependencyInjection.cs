using Core.Abstractions;
using Persistence.Models.Context;
using Infrastructure.Options;
using Infrastructure.Behaviors;
using Infrastructure.Implementations;
using MediatR;
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

        services.AddDbContext<LoyaltyHubDbContext>(options =>
            options.UseNpgsql(databaseOptions.ConnectionString));

        services.AddSingleton(databaseOptions);

        services.AddScoped<ICustomerTierRepo, CustomerTierRepository>();
        services.AddScoped<ICustomerTierMutationStore, CustomerTierMutationStore>();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly); // Infrastructure (behaviors)
            cfg.RegisterServicesFromAssembly(typeof(ICustomerTierRepo).Assembly);   // Core (handlers)
        });

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(SaveChangesBehavior<,>));

        return services;
    }
}