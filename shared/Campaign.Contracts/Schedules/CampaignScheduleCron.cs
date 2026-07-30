using Campaign.Contracts.Constants;

namespace Campaign.Contracts.Schedules;

public sealed class CampaignScheduleCron
{
    private CampaignScheduleCron(
        int hour,
        int minute,
        bool isDaily,
        IReadOnlyList<DayOfWeek> daysOfWeek,
        string canonicalCron)
    {
        Hour = hour;
        Minute = minute;
        IsDaily = isDaily;
        DaysOfWeek = daysOfWeek;
        CanonicalCron = canonicalCron;
    }

    public int Hour { get; }
    public int Minute { get; }
    public bool IsDaily { get; }
    public IReadOnlyList<DayOfWeek> DaysOfWeek { get; }
    public string CanonicalCron { get; }

    public static CampaignScheduleCron Parse(string input)
    {
        if (TryParse(input, out var schedule) && schedule is not null)
        {
            return schedule;
        }

        throw new FormatException($"Invalid campaign schedule cron expression: '{input}'.");
    }

    public static bool TryParse(string? input, out CampaignScheduleCron? schedule)
    {
        schedule = null;

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var parts = input.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 6)
        {
            return false;
        }

        if (parts[0] != "0")
        {
            return false;
        }

        if (!int.TryParse(parts[1], out var minute) || minute < 0 || minute > 59 || parts[1] != minute.ToString())
        {
            return false;
        }

        if (!int.TryParse(parts[2], out var hour) || hour < 0 || hour > 23 || parts[2] != hour.ToString())
        {
            return false;
        }

        if (parts[3] == "*" && parts[4] == "*" && parts[5] == "?")
        {
            schedule = new CampaignScheduleCron(
                hour,
                minute,
                true,
                Array.Empty<DayOfWeek>(),
                $"0 {minute} {hour} * * ?");
            return true;
        }

        if (parts[3] == "?" && parts[4] == "*")
        {
            var tokens = parts[5].Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0 || tokens.Length != parts[5].Split(',').Length)
            {
                return false;
            }

            var daysOfWeek = new List<DayOfWeek>(tokens.Length);
            int lastIndex = -1;

            foreach (var token in tokens)
            {
                int index = IndexOfCanonicalDay(token);
                if (index == -1 || index <= lastIndex)
                {
                    return false;
                }

                lastIndex = index;
                daysOfWeek.Add(MapToDayOfWeek(index));
            }

            schedule = new CampaignScheduleCron(
                hour,
                minute,
                false,
                daysOfWeek,
                $"0 {minute} {hour} ? * {string.Join(',', tokens)}");
            return true;
        }

        return false;
    }

    public bool TryGetOccurrences(
        DateTime startDateUtc,
        DateTime endDateUtc,
        int durationHour,
        out IReadOnlyList<CampaignScheduleOccurrence> occurrences,
        out string? errorCode,
        int maxSessions = CampaignScheduleLimits.MaximumGeneratedSessions)
    {
        occurrences = Array.Empty<CampaignScheduleOccurrence>();
        errorCode = null;

        if (durationHour <= 0)
        {
            errorCode = "CAMPAIGN_DURATION_INVALID";
            return false;
        }

        if (endDateUtc <= startDateUtc)
        {
            errorCode = "CAMPAIGN_DATE_RANGE_INVALID";
            return false;
        }

        var list = new List<CampaignScheduleOccurrence>();
        var currentDate = startDateUtc.Date;
        var endLimitDate = endDateUtc.Date;

        while (currentDate <= endLimitDate)
        {
            if (IsDaily || DaysOfWeek.Contains(currentDate.DayOfWeek))
            {
                var candidateStart = DateTime.SpecifyKind(currentDate.AddHours(Hour).AddMinutes(Minute), DateTimeKind.Utc);
                if (candidateStart >= startDateUtc && candidateStart < endDateUtc)
                {
                    var candidateEnd = candidateStart.AddHours(durationHour);
                    var actualEnd = candidateEnd < endDateUtc ? candidateEnd : endDateUtc;

                    if (list.Count > 0 && candidateStart < list[^1].SessionEndUtc)
                    {
                        errorCode = "CAMPAIGN_SCHEDULE_INVALID";
                        return false;
                    }

                    list.Add(new CampaignScheduleOccurrence(candidateStart, actualEnd));

                    if (list.Count > maxSessions)
                    {
                        errorCode = "CAMPAIGN_SCHEDULE_TOO_LARGE";
                        return false;
                    }
                }
            }

            currentDate = currentDate.AddDays(1);
        }

        if (list.Count == 0)
        {
            errorCode = "CAMPAIGN_SCHEDULE_EMPTY";
            return false;
        }

        occurrences = list;
        return true;
    }

    private static int IndexOfCanonicalDay(string token)
    {
        for (int i = 0; i < CampaignScheduleDays.CanonicalOrder.Count; i++)
        {
            if (string.Equals(CampaignScheduleDays.CanonicalOrder[i], token, StringComparison.Ordinal))
            {
                return i;
            }
        }

        return -1;
    }

    private static DayOfWeek MapToDayOfWeek(int index)
    {
        return index switch
        {
            0 => DayOfWeek.Monday,
            1 => DayOfWeek.Tuesday,
            2 => DayOfWeek.Wednesday,
            3 => DayOfWeek.Thursday,
            4 => DayOfWeek.Friday,
            5 => DayOfWeek.Saturday,
            6 => DayOfWeek.Sunday,
            _ => throw new ArgumentOutOfRangeException(nameof(index))
        };
    }
}
