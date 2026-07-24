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

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var databaseOptions = DatabaseOptions.FromConfiguration(configuration);
        var voucherPoolImportOptions =
            VoucherPoolImportOptions.FromConfiguration(configuration);

        services.AddDbContext<LoyaltyHubDbContext>(options =>
            options.UseNpgsql(databaseOptions.ConnectionString));

        services.AddSingleton(databaseOptions);
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

        services.AddScoped<ICustomerTierRepository, CustomerTierRepository>();
        services.AddScoped<ICustomerTierMutationStore, CustomerTierMutationStore>();
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

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly); // Infrastructure (behaviors)
            cfg.RegisterServicesFromAssembly(typeof(ICustomerTierRepository).Assembly); // Core (handlers)
        });

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(SaveChangesBehavior<,>));

        return services;
    }
}
