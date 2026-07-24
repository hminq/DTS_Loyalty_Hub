using DotNetEnv;
using Infrastructure;
using Microsoft.Extensions.Configuration;
using Quartz;
using Scheduler.Jobs;
using Scheduler.Options;

var builder = Host.CreateApplicationBuilder(args);

// Local Scheduler only loads worker/Scheduler/.env. Deployed environments use
// variables injected by Docker or the server runtime.
if (builder.Environment.IsDevelopment())
{
    var envPath = new[]
        {
            Path.Combine(builder.Environment.ContentRootPath, ".env"),
            Path.Combine(builder.Environment.ContentRootPath, "Scheduler", ".env"),
            Path.Combine(builder.Environment.ContentRootPath, "worker", "Scheduler", ".env"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".env")
        }
        .Select(Path.GetFullPath)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .Where(path => string.Equals(
            Path.GetFileName(Path.GetDirectoryName(path)),
            "Scheduler",
            StringComparison.OrdinalIgnoreCase))
        .FirstOrDefault(File.Exists);

    if (envPath is not null)
    {
        Console.WriteLine($"Loading local Scheduler environment from '{envPath}'.");
        Env.Load(envPath);
        builder.Configuration.AddEnvironmentVariables();
    }
}

var scheduleOptions = TierExpirationScheduleOptions.FromConfiguration(builder.Configuration);
var voucherPoolScheduleOptions =
    VoucherPoolProvisioningScheduleOptions.FromConfiguration(builder.Configuration);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton(scheduleOptions);
builder.Services.AddSingleton(voucherPoolScheduleOptions);
builder.Services.AddQuartz(quartz =>
{
    var tierExpirationJobKey = new JobKey(nameof(ProcessExpiredCustomerTiersJob));

    quartz.AddJob<ProcessExpiredCustomerTiersJob>(
        job => job.WithIdentity(tierExpirationJobKey));
    quartz.AddTrigger(trigger => trigger
        .ForJob(tierExpirationJobKey)
        .WithIdentity($"{tierExpirationJobKey.Name}-trigger")
        .WithCronSchedule(
            scheduleOptions.Cron,
            cron => cron
                .InTimeZone(TimeZoneInfo.FindSystemTimeZoneById(scheduleOptions.TimeZone))
                .WithMisfireHandlingInstructionDoNothing()));

    var voucherPoolJobKey = new JobKey(nameof(ProcessVoucherPoolProvisioningJob));

    quartz.AddJob<ProcessVoucherPoolProvisioningJob>(
        job => job.WithIdentity(voucherPoolJobKey));
    quartz.AddTrigger(trigger => trigger
        .ForJob(voucherPoolJobKey)
        .WithIdentity($"{voucherPoolJobKey.Name}-trigger")
        .WithCronSchedule(
            voucherPoolScheduleOptions.Cron,
            cron => cron
                .InTimeZone(TimeZoneInfo.FindSystemTimeZoneById(
                    voucherPoolScheduleOptions.TimeZone))
                .WithMisfireHandlingInstructionDoNothing()));
});
builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});

builder.Services.AddInfrastructure(builder.Configuration);

var host = builder.Build();
host.Run();
