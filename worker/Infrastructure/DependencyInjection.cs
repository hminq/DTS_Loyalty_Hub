using Core.Abstractions;
using Persistence.Models.Context;
using Infrastructure.Options;
using Infrastructure.Behaviors;
using Infrastructure.Implementations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Infrastructure.RabbitMq;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services
            .AddWorkerPersistence(configuration)
            .AddWorkerPersistenceBehaviors()
            .AddCustomerTierInfrastructure()
            .AddVoucherPoolInfrastructure(configuration)
            .AddCampaignSessionLifecycle();
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
            cfg.RegisterServicesFromAssembly(typeof(ICustomerTierRepository).Assembly);
        });
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(SaveChangesBehavior<,>));

        return services;
    }

    public static IServiceCollection AddCustomerTierInfrastructure(
        this IServiceCollection services)
    {
        services.AddScoped<ICustomerTierRepository, CustomerTierRepository>();
        services.AddScoped<ICustomerTierMutationStore, CustomerTierMutationStore>();

        return services;
    }

    public static IServiceCollection AddVoucherPoolInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var voucherPoolImportOptions =
            VoucherPoolImportOptions.FromConfiguration(configuration);

        services.AddSingleton(voucherPoolImportOptions);
        services.AddSingleton<IAmazonS3>(provider =>
        {
            var importOptions = provider.GetRequiredService<VoucherPoolImportOptions>();
            return new AmazonS3Client(
                new BasicAWSCredentials(
                    importOptions.AccessKeyId,
                    importOptions.SecretAccessKey),
                RegionEndpoint.GetBySystemName(importOptions.Region));
        });

        services.AddScoped<VoucherPoolProvisioningStore>();
        services.AddScoped<IVoucherPoolProvisioningRepository>(
            provider => provider.GetRequiredService<VoucherPoolProvisioningStore>());
        services.AddScoped<IVoucherPoolMutationStore>(
            provider => provider.GetRequiredService<VoucherPoolProvisioningStore>());
        services.AddScoped<IVoucherPoolImportStore>(
            provider => provider.GetRequiredService<VoucherPoolProvisioningStore>());
        services.AddScoped<IVoucherPoolImportFileReader, S3VoucherPoolImportFileReader>();
        services.AddSingleton<IVoucherCodeGenerator, CryptographicVoucherCodeGenerator>();
        services.AddSingleton<IVoucherPoolGenerationFailureClassifier, VoucherPoolGenerationFailureClassifier>();

        return services;
    }

    public static IServiceCollection AddCampaignSessionLifecycle(
        this IServiceCollection services)
    {
        services.AddScoped<ICampaignSessionLifecycleStore, CampaignSessionLifecycleStore>();
        return services;
    }

    public static IServiceCollection AddOutboxPublishing(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = RabbitMqPublisherOptions.FromConfiguration(configuration);

        services.AddSingleton(options);
        services.AddScoped<IOutboxDispatchStore, OutboxDispatchStore>();
        services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();

        return services;
    }

    public static IServiceCollection AddCampaignEventProcessing(
        this IServiceCollection services)
    {
        services.AddScoped<ICampaignEventPreparationStore, CampaignEventPreparationStore>();
        services.AddScoped<ICampaignRewardExecutionStore, CampaignRewardExecutionStore>();
        services.AddScoped<IEventProcessingFinalizationStore, EventProcessingFinalizationStore>();
        services.AddSingleton<ICampaignProcessingScopeExecutor, CampaignProcessingScopeExecutor>();
        services.AddSingleton<Core.Services.CampaignEventProcessingCoordinator>();
        services.AddSingleton<Core.Services.CampaignEventDeliveryProcessor>();
        services.AddSingleton<
            ICustomerAccountRegisteredEventValidator,
            Core.Services.CustomerAccountRegisteredEventValidator>();
        services.AddSingleton<
            ICustomerAccountRegisteredConditionEvaluator,
            Core.Services.CustomerAccountRegisteredConditionEvaluator>();
        services.AddSingleton<
            IIssuePointActionConfigParser,
            Core.Services.IssuePointActionConfigParser>();

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
}
