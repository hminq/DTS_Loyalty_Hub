using DotNetEnv;
using Infrastructure;
using Microsoft.Extensions.Configuration;
using Quartz;
using Scheduler.Jobs;
using Scheduler.Options;

var builder = Host.CreateApplicationBuilder(args);

// Local Scheduler runs from either worker/ or worker/Scheduler/. Load worker/.env
// only in Development; deployed environments continue to use injected variables.
if (builder.Environment.IsDevelopment())
{
    var envPath = new[]
        {
            Path.Combine(builder.Environment.ContentRootPath, ".env"),
            Path.Combine(builder.Environment.ContentRootPath, "..", ".env")
        }
        .Select(Path.GetFullPath)
        .FirstOrDefault(File.Exists);

    if (envPath is not null)
    {
        Env.Load(envPath);
        builder.Configuration.AddEnvironmentVariables();
    }
}

var scheduleOptions = builder.Configuration
    .GetRequiredSection(TierExpirationScheduleOptions.SectionName)
    .Get<TierExpirationScheduleOptions>()
    ?? throw new InvalidOperationException(
        $"Missing configuration section: {TierExpirationScheduleOptions.SectionName}");
scheduleOptions.Validate();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddQuartz(quartz =>
{
    var jobKey = new JobKey(nameof(ProcessExpiredCustomerTiersJob));

    quartz.AddJob<ProcessExpiredCustomerTiersJob>(job => job.WithIdentity(jobKey));
    quartz.AddTrigger(trigger => trigger
        .ForJob(jobKey)
        .WithIdentity($"{jobKey.Name}-trigger")
        .WithCronSchedule(
            scheduleOptions.Cron,
            cron => cron
                .InTimeZone(TimeZoneInfo.FindSystemTimeZoneById(scheduleOptions.TimeZone))
                .WithMisfireHandlingInstructionDoNothing()));
});
builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});

builder.Services.AddInfrastructure(builder.Configuration);

var host = builder.Build();
host.Run();
