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
            .AddCampaignEventProcessing(configuration)
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
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var cacheOptions = EventDefinitionCacheOptions.FromConfiguration(configuration);
        services.AddSingleton(cacheOptions);

        services.AddSingleton<IVersionedEnvelopeParser, Core.Services.VersionedEnvelopeParser>();
        services.AddScoped<IEventDefinitionStore, EfEventDefinitionStore>();
        services.AddSingleton<IEventDefinitionProvider, BoundedEventDefinitionCache>();
        services.AddSingleton<Core.Services.GenericCampaignEventFactory>();
        services.AddSingleton<Core.Services.GenericCampaignTargetResolver>();

        services.AddScoped<ICampaignEventPreparationStore, CampaignEventPreparationStore>();
        services.AddScoped<ICampaignRewardExecutionStore, CampaignRewardExecutionStore>();
        services.AddScoped<Core.Services.CustomerTierProgressionService>();
        services.AddScoped<IIssuePointExecutionStore, IssuePointExecutionStore>();
        services.AddScoped<IEventProcessingFinalizationStore, EventProcessingFinalizationStore>();
        services.AddSingleton<ICampaignProcessingScopeExecutor, CampaignProcessingScopeExecutor>();
        services.AddSingleton<Core.Services.CampaignEventProcessingCoordinator>();
        services.AddSingleton<Core.Services.CampaignEventDeliveryProcessor>();
        services.AddSingleton(CampaignActionCatalog.BuiltIn);
        services.AddScoped<
            ICampaignActionExecutor,
            Core.Services.IssuePointActionExecutor>();
        services.AddScoped<
            ICampaignActionExecutorRegistry,
            Core.Services.CampaignActionExecutorRegistry>();
        services.AddSingleton<CampaignActionBindingParser>();
        services.AddSingleton<Core.Services.GenericCampaignConditionEvaluator>();

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

        services.GetRequiredService<IEventDefinitionProvider>();
        services.GetRequiredService<IVersionedEnvelopeParser>();
        services.GetRequiredService<Core.Services.GenericCampaignEventFactory>();
        using var scope = services.CreateScope();
        scope.ServiceProvider.GetRequiredService<ICampaignActionExecutorRegistry>();

        return services;
    }
}
