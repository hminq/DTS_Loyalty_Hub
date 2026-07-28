using DotNetEnv;
using Consumer;
using Infrastructure;
using Microsoft.Extensions.Configuration;

var builder = Host.CreateApplicationBuilder(args);

if (builder.Environment.IsDevelopment())
{
    var envPath = new[]
        {
            Path.Combine(builder.Environment.ContentRootPath, ".env"),
            Path.Combine(builder.Environment.ContentRootPath, "Consumer", ".env"),
            Path.Combine(builder.Environment.ContentRootPath, "worker", "Consumer", ".env"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".env")
        }
        .Select(Path.GetFullPath)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .Where(path => string.Equals(
            Path.GetFileName(Path.GetDirectoryName(path)),
            "Consumer",
            StringComparison.OrdinalIgnoreCase))
        .FirstOrDefault(File.Exists);

    if (envPath is not null)
    {
        Console.WriteLine($"Loading local Consumer environment from '{envPath}'.");
        Env.Load(envPath);
        builder.Configuration.AddEnvironmentVariables();
    }
}

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddWorkerPersistence(builder.Configuration);
builder.Services.AddWorkerPersistenceBehaviors();
builder.Services.AddCampaignEventProcessing();
builder.Services.AddRabbitMqCampaignConsumer(builder.Configuration);
builder.Services.AddHostedService<ConsumerWorker>();

var host = builder.Build();
host.Run();
