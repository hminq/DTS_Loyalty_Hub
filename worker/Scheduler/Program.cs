using Scheduler;
using Quartz;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddQuartz();
builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});
builder.Services.AddHostedService<SchedulerWorker>();

var host = builder.Build();
host.Run();
