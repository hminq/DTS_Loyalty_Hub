using Microsoft.Extensions.Configuration;

namespace Infrastructure.Options;

public sealed class DatabaseOptions
{
    private DatabaseOptions(string connectionString)
    {
        ConnectionString = connectionString;
    }

    public string ConnectionString { get; }

    public static DatabaseOptions FromConfiguration(IConfiguration configuration)
    {
        var connectionString = configuration["DATABASE_URL"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Missing required configuration value: DATABASE_URL");
        }

        return new DatabaseOptions(connectionString);
    }
}
