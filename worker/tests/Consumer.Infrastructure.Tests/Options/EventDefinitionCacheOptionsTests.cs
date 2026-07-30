using Consumer.Infrastructure.Options;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace Consumer.Infrastructure.Tests.Options;

public sealed class EventDefinitionCacheOptionsTests
{
    [Fact]
    public void FromConfiguration_ValidValues_ParsesSuccessfully()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["EVENT_DEFINITION_CACHE_MAX_ENTRIES"] = "500",
                ["EVENT_DEFINITION_CACHE_TTL_SECONDS"] = "300"
            })
            .Build();

        var options = EventDefinitionCacheOptions.FromConfiguration(config);

        options.MaxEntries.Should().Be(500);
        options.TtlSeconds.Should().Be(300);
    }

    [Theory]
    [InlineData(null, "300")]
    [InlineData("", "300")]
    [InlineData("0", "300")]
    [InlineData("-10", "300")]
    [InlineData("10001", "300")]
    [InlineData("abc", "300")]
    [InlineData("100", null)]
    [InlineData("100", "")]
    [InlineData("100", "0")]
    [InlineData("100", "-5")]
    [InlineData("100", "86401")]
    public void FromConfiguration_MissingOrInvalid_ThrowsInvalidOperationException(
        string? maxEntries,
        string? ttlSeconds)
    {
        var dict = new Dictionary<string, string?>();
        if (maxEntries is not null) dict["EVENT_DEFINITION_CACHE_MAX_ENTRIES"] = maxEntries;
        if (ttlSeconds is not null) dict["EVENT_DEFINITION_CACHE_TTL_SECONDS"] = ttlSeconds;

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(dict)
            .Build();

        var act = () => EventDefinitionCacheOptions.FromConfiguration(config);

        act.Should().Throw<InvalidOperationException>();
    }
}
