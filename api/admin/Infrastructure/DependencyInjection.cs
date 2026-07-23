using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Core.Abstractions;
using Infrastructure.Auth;
using Infrastructure.Implementations;
using Persistence.Models.Context;
using Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using Infrastructure.Auditing;
using Infrastructure.Behaviors;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var databaseOptions = DatabaseOptions.FromConfiguration(configuration);
        var jwtOptions = JwtOptions.FromConfiguration(configuration);
        var s3Options = S3Options.FromConfiguration(configuration);

        services.AddDbContext<LoyaltyHubDbContext>(options =>
            options.UseNpgsql(databaseOptions.ConnectionString));

        services.AddSingleton(databaseOptions);
        services.AddSingleton(jwtOptions);
        services.AddSingleton(s3Options);
        services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client(
            new BasicAWSCredentials(s3Options.AccessKeyId, s3Options.SecretAccessKey),
            RegionEndpoint.GetBySystemName(s3Options.Region)));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAdminRepository, AdminRepository>();
        services.AddScoped<IAdminSessionRepository, AdminSessionRepository>();
        services.AddScoped<IAdminPermissionChecker, AdminPermissionChecker>();
        services.AddScoped<IAdminUserRepository, AdminUserRepository>();
        services.AddScoped<ICustomerUserRepository, CustomerUserRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRoleReader, RoleReader>();
        services.AddScoped<ITierRepository, TierRepository>();
        services.AddScoped<IVoucherDefinitionRepository, VoucherDefinitionRepository>();
        services.AddScoped<ICustomerVoucherRepository, CustomerVoucherRepository>();
        services.AddScoped<IPasswordVerifier, PasswordVerifier>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IAccessTokenService, JwtAccessTokenService>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<INotificationEventTypeRepository, NotificationEventTypeRepository>();
        services.AddScoped<INotificationTemplateRepository, NotificationTemplateRepository>();
        services.AddScoped<INotificationLogRepository, NotificationLogRepository>();
        services.AddScoped<IAuditLogWriter, AuditLogWriter>();
        services.AddScoped<IBannerStorage, S3BannerStorage>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(SaveChangesBehavior<,>));
        
        return services;
    }
}
