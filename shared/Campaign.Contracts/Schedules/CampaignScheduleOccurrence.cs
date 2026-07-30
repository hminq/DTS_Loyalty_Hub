namespace Campaign.Contracts.Schedules;

public readonly record struct CampaignScheduleOccurrence(
    DateTime SessionStartUtc,
    DateTime SessionEndUtc);
