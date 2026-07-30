using Campaign.Contracts.Schedules;
using FluentAssertions;

namespace Core.Tests.Entities;

public sealed class CampaignScheduleCronTests
{
    [Theory]
    [InlineData("0 5 9 * * ?", "0 5 9 * * ?")]
    [InlineData("0 5 9 ? * MON,WED", "0 5 9 ? * MON,WED")]
    [InlineData("0 5 9 29 * ?", "0 5 9 29 * ?")]
    [InlineData("0 5 9 1,5,6,31 * ?", "0 5 9 1,5,6,31 * ?")]
    [InlineData("0 5 9 L * ?", "0 5 9 L * ?")]
    public void TryParse_SupportedPattern_ReturnsCanonicalSchedule(
        string input,
        string expected)
    {
        var parsed = CampaignScheduleCron.TryParse(input, out var schedule);

        parsed.Should().BeTrue();
        schedule.Should().NotBeNull();
        schedule!.CanonicalCron.Should().Be(expected);
    }

    [Theory]
    [InlineData("0 5 9 0 * ?")]
    [InlineData("0 5 9 32 * ?")]
    [InlineData("0 5 9 01 * ?")]
    [InlineData("0 5 9 1,01 * ?")]
    [InlineData("0 5 9 1,1 * ?")]
    [InlineData("0 5 9 5,1 * ?")]
    [InlineData("0 5 9 1,,5 * ?")]
    [InlineData("0 5 9 L-1 * ?")]
    [InlineData("0 5 9 15 * MON")]
    public void TryParse_UnsupportedMonthlyPattern_ReturnsFalse(string input)
    {
        CampaignScheduleCron.TryParse(input, out var schedule).Should().BeFalse();
        schedule.Should().BeNull();
    }

    [Fact]
    public void TryGetOccurrences_MultipleMonthlyDays_ReturnsEverySelectedCalendarDay()
    {
        var schedule = CampaignScheduleCron.Parse("0 0 8 1,5,6 * ?");

        var valid = schedule.TryGetOccurrences(
            new DateTime(2025, 1, 4, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 2, 7, 0, 0, 0, DateTimeKind.Utc),
            1,
            out var occurrences,
            out var errorCode);

        valid.Should().BeTrue();
        errorCode.Should().BeNull();
        schedule.DaysOfMonth.Should().Equal(1, 5, 6);
        occurrences.Select(item => item.SessionStartUtc).Should().Equal(
            new DateTime(2025, 1, 5, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 1, 6, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 2, 1, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 2, 5, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 2, 6, 8, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void TryGetOccurrences_MonthlyDay29_SkipsNonLeapFebruary()
    {
        var schedule = CampaignScheduleCron.Parse("0 0 8 29 * ?");

        var valid = schedule.TryGetOccurrences(
            new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 4, 1, 0, 0, 0, DateTimeKind.Utc),
            1,
            out var occurrences,
            out var errorCode);

        valid.Should().BeTrue();
        errorCode.Should().BeNull();
        occurrences.Select(item => item.SessionStartUtc).Should().Equal(
            new DateTime(2025, 1, 29, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 3, 29, 8, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void TryGetOccurrences_MonthlyDay31_SkipsShortMonths()
    {
        var schedule = CampaignScheduleCron.Parse("0 30 10 31 * ?");

        var valid = schedule.TryGetOccurrences(
            new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 5, 1, 0, 0, 0, DateTimeKind.Utc),
            1,
            out var occurrences,
            out var errorCode);

        valid.Should().BeTrue();
        errorCode.Should().BeNull();
        occurrences.Select(item => item.SessionStartUtc).Should().Equal(
            new DateTime(2025, 1, 31, 10, 30, 0, DateTimeKind.Utc),
            new DateTime(2025, 3, 31, 10, 30, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void TryGetOccurrences_MultipleMonthlyDays_SkipsOnlyMissingDay()
    {
        var schedule = CampaignScheduleCron.Parse("0 0 8 1,31 * ?");

        var valid = schedule.TryGetOccurrences(
            new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 3, 2, 0, 0, 0, DateTimeKind.Utc),
            1,
            out var occurrences,
            out var errorCode);

        valid.Should().BeTrue();
        errorCode.Should().BeNull();
        occurrences.Select(item => item.SessionStartUtc).Should().Equal(
            new DateTime(2025, 2, 1, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 3, 1, 8, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void TryGetOccurrences_LastDayOfMonth_UsesLeapDay()
    {
        var schedule = CampaignScheduleCron.Parse("0 15 7 L * ?");

        var valid = schedule.TryGetOccurrences(
            new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2024, 4, 1, 0, 0, 0, DateTimeKind.Utc),
            1,
            out var occurrences,
            out var errorCode);

        valid.Should().BeTrue();
        errorCode.Should().BeNull();
        occurrences.Select(item => item.SessionStartUtc).Should().Equal(
            new DateTime(2024, 1, 31, 7, 15, 0, DateTimeKind.Utc),
            new DateTime(2024, 2, 29, 7, 15, 0, DateTimeKind.Utc),
            new DateTime(2024, 3, 31, 7, 15, 0, DateTimeKind.Utc));
    }
}
