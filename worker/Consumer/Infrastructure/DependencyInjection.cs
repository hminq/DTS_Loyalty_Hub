using Consumer.Core.Abstractions;
using Persistence.Models.Context;
using Consumer.Infrastructure.Options;
using Consumer.Infrastructure.Behaviors;
using Consumer.Infrastructure.Implementations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Consumer.Infrastructure.RabbitMq;
using Campaign.Contracts.Definitions;
using Campaign.Contracts.Actions;
using Campaign.Contracts.Conditions;
using System;

namespace Consumer.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services
            .AddWorkerPersistence(configuration)
            .AddWorkerPersistenceBehaviors()
            .AddCampaignEventProcessing()
            .AddRabbitMqCampaignConsumer(configuration);
    }

    public static IServiceCollection AddWorkerPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var databaseOptions = DatabaseOptions.FromConfiguration(configuration);

        services.AddDbContext<LoyaltyHubDbContext>(options =>
            options.UseNpgsql(databaseOptions.ConnectionString));
        services.AddSingleton(databaseOptions);

        return services;
    }

    public static IServiceCollection AddWorkerPersistenceBehaviors(
        this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(IWriteRequest).Assembly);
        });
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(SaveChangesBehavior<,>));

        return services;
    }

    public static IServiceCollection AddCampaignEventProcessing(
        this IServiceCollection services)
    {
        services.AddScoped<ICampaignEventPreparationStore, CampaignEventPreparationStore>();
        services.AddScoped<ICampaignRewardExecutionStore, CampaignRewardExecutionStore>();
        services.AddScoped<IIssuePointExecutionStore, IssuePointExecutionStore>();
        services.AddScoped<IEventProcessingFinalizationStore, EventProcessingFinalizationStore>();
        services.AddSingleton<ICampaignProcessingScopeExecutor, CampaignProcessingScopeExecutor>();
        services.AddSingleton<Core.Services.CampaignEventProcessingCoordinator>();
        services.AddSingleton<Core.Services.CampaignEventDeliveryProcessor>();
        services.AddSingleton(CampaignDefinitionCatalog.BuiltIn);
        services.AddSingleton<
            ICustomerAccountRegisteredEventValidator,
            Core.Services.CustomerAccountRegisteredEventValidator>();
        services.AddSingleton<
            ICampaignEventRuntimeDefinition,
            Core.Services.CustomerAccountRegisteredEventRuntimeDefinition>();
        services.AddSingleton<
            ICampaignEventRuntimeRegistry,
            Core.Services.CampaignEventRuntimeRegistry>();
        services.AddScoped<
            ICampaignActionExecutor,
            Core.Services.IssuePointActionExecutor>();
        services.AddScoped<
            ICampaignActionExecutorRegistry,
            Core.Services.CampaignActionExecutorRegistry>();
        services.AddSingleton<CampaignActionBindingParser>();
        services.AddSingleton<CampaignConditionParser>();
        services.AddSingleton<CampaignConditionEvaluator>();
        services.AddSingleton<CampaignConditionCompatibilityAnalyzer>();

        return services;
    }

    public static IServiceCollection AddRabbitMqCampaignConsumer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = RabbitMqConsumerOptions.FromConfiguration(configuration);

        services.AddSingleton(options);
        services.AddSingleton<
            ICampaignEventDeliverySource,
            RabbitMqCampaignEventDeliverySource>();

        return services;
    }

    public static IServiceProvider ValidateCampaignEventProcessing(
        this IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.GetRequiredService<ICampaignEventRuntimeRegistry>();
        using var scope = services.CreateScope();
        scope.ServiceProvider.GetRequiredService<ICampaignActionExecutorRegistry>();

        return services;
    }
}
