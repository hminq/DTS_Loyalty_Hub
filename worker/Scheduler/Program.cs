using Infrastructure;
using Scheduler;
using Quartz;
using DotNetEnv;

var envPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", ".env"));

// Load local environment variables from the Customer API root directory.
if (File.Exists(envPath))
{
    DotNetEnv.Env.Load(envPath);
}

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddQuartz();
builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHostedService<SchedulerWorker>();
builder.Services.AddHostedService<TierExpirationSchedulerWorker>();

var host = builder.Build();
host.Run();