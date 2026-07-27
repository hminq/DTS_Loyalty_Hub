using Campaign.Contracts.Constants;
using Core.Entities.Constants;
using Core.Exceptions;

namespace Core.Entities;

public sealed class Campaign
{
    private const int MaximumNameLength = 200;
    private const int MaximumScheduleCronLength = 100;

    private Campaign(
        Guid campaignId,
        string campaignName,
        string? description,
        string? bannerImageUrl,
        string eventType,
        string condition,
        DateTime startDate,
        DateTime endDate,
        string scheduleCron,
        int durationHour,
        int? userLimitTotal,
        int? userLimitSession,
        string status,
        DateTime createdAt,
        DateTime updatedAt)
    {
        CampaignId = campaignId;
        CampaignName = campaignName;
        Description = description;
        BannerImageUrl = bannerImageUrl;
        EventType = eventType;
        Condition = condition;
        StartDate = startDate;
        EndDate = endDate;
        ScheduleCron = scheduleCron;
        DurationHour = durationHour;
        UserLimitTotal = userLimitTotal;
        UserLimitSession = userLimitSession;
        Status = status;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public Guid CampaignId { get; }
    public string CampaignName { get; private set; }
    public string? Description { get; private set; }
    public string? BannerImageUrl { get; private set; }
    public string EventType { get; private set; }
    public string Condition { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public string ScheduleCron { get; private set; }
    public int DurationHour { get; private set; }
    public int? UserLimitTotal { get; private set; }
    public int? UserLimitSession { get; private set; }
    public string Status { get; }
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; private set; }

    public static Campaign Create(
        string campaignName,
        string? description,
        string? bannerImageUrl,
        string eventType,
        string condition,
        DateTime startDate,
        DateTime endDate,
        string scheduleCron,
        int durationHour,
        int? userLimitTotal,
        int? userLimitSession,
        DateTime now)
    {
        Validate(
            campaignName,
            bannerImageUrl,
            eventType,
            condition,
            startDate,
            endDate,
            scheduleCron,
            durationHour,
            userLimitTotal,
            userLimitSession);

        return new Campaign(
            Guid.NewGuid(),
            campaignName.Trim(),
            NormalizeOptional(description),
            NormalizeOptional(bannerImageUrl),
            eventType,
            condition,
            startDate,
            endDate,
            scheduleCron.Trim(),
            durationHour,
            userLimitTotal,
            userLimitSession,
            CampaignStatuses.Draft,
            now,
            now);
    }

    public static Campaign Restore(
        Guid campaignId,
        string campaignName,
        string? description,
        string? bannerImageUrl,
        string eventType,
        string condition,
        DateTime startDate,
        DateTime endDate,
        string scheduleCron,
        int durationHour,
        int? userLimitTotal,
        int? userLimitSession,
        string status,
        DateTime createdAt,
        DateTime updatedAt)
    {
        if (campaignId == Guid.Empty)
        {
            throw ValidationError("CAMPAIGN_ID_REQUIRED");
        }

        return new Campaign(
            campaignId,
            campaignName,
            NormalizeOptional(description),
            NormalizeOptional(bannerImageUrl),
            eventType,
            condition,
            startDate,
            endDate,
            scheduleCron,
            durationHour,
            userLimitTotal,
            userLimitSession,
            status,
            createdAt,
            updatedAt);
    }

    public void Update(
        string campaignName,
        string? description,
        string? bannerImageUrl,
        string eventType,
        string condition,
        DateTime startDate,
        DateTime endDate,
        string scheduleCron,
        int durationHour,
        int? userLimitTotal,
        int? userLimitSession,
        DateTime updatedAt)
    {
        EnsureDraft();
        Validate(
            campaignName,
            bannerImageUrl,
            eventType,
            condition,
            startDate,
            endDate,
            scheduleCron,
            durationHour,
            userLimitTotal,
            userLimitSession);

        CampaignName = campaignName.Trim();
        Description = NormalizeOptional(description);
        BannerImageUrl = NormalizeOptional(bannerImageUrl);
        EventType = eventType;
        Condition = condition;
        StartDate = startDate;
        EndDate = endDate;
        ScheduleCron = scheduleCron.Trim();
        DurationHour = durationHour;
        UserLimitTotal = userLimitTotal;
        UserLimitSession = userLimitSession;
        UpdatedAt = updatedAt;
    }

    public void EnsureDraft()
    {
        if (!Status.Equals(CampaignStatuses.Draft, StringComparison.Ordinal))
        {
            throw new DomainException("CAMPAIGN_NOT_DRAFT", DomainErrorType.Conflict);
        }
    }

    private static void Validate(
        string campaignName,
        string? bannerImageUrl,
        string eventType,
        string condition,
        DateTime startDate,
        DateTime endDate,
        string scheduleCron,
        int durationHour,
        int? userLimitTotal,
        int? userLimitSession)
    {
        if (string.IsNullOrWhiteSpace(campaignName))
        {
            throw ValidationError("CAMPAIGN_NAME_REQUIRED");
        }

        if (campaignName.Trim().Length > MaximumNameLength)
        {
            throw ValidationError("CAMPAIGN_NAME_TOO_LONG");
        }

        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw ValidationError("CAMPAIGN_EVENT_TYPE_INVALID");
        }

        if (string.IsNullOrWhiteSpace(condition))
        {
            throw ValidationError("CAMPAIGN_CONDITION_INVALID");
        }

        if (!string.IsNullOrWhiteSpace(bannerImageUrl) &&
            !bannerImageUrl.Trim().StartsWith(
                BannerUploadTypes.CampaignBannerPrefix,
                StringComparison.Ordinal))
        {
            throw ValidationError("CAMPAIGN_BANNER_IMAGE_KEY_INVALID");
        }

        if (endDate <= startDate)
        {
            throw ValidationError("CAMPAIGN_DATE_RANGE_INVALID");
        }

        if (string.IsNullOrWhiteSpace(scheduleCron) ||
            scheduleCron.Trim().Length > MaximumScheduleCronLength)
        {
            throw ValidationError("CAMPAIGN_SCHEDULE_INVALID");
        }

        if (durationHour <= 0)
        {
            throw ValidationError("CAMPAIGN_DURATION_INVALID");
        }

        if (userLimitTotal is < 0 ||
            userLimitSession is < 0 ||
            userLimitTotal.HasValue &&
            userLimitSession.HasValue &&
            userLimitSession > userLimitTotal)
        {
            throw ValidationError("CAMPAIGN_LIMIT_INVALID");
        }
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static DomainException ValidationError(string code)
    {
        return new DomainException(code, DomainErrorType.Validation);
    }
}
