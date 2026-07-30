using Campaign.Contracts.Constants;
using Core.Exceptions;

namespace Core.Entities;

public sealed class CampaignSession
{
    private CampaignSession(
        Guid campaignSessionId,
        Guid campaignId,
        DateTime sessionStart,
        DateTime sessionEnd,
        string status,
        DateTime createdAt,
        DateTime? endedAt)
    {
        CampaignSessionId = campaignSessionId;
        CampaignId = campaignId;
        SessionStart = sessionStart;
        SessionEnd = sessionEnd;
        Status = status;
        CreatedAt = createdAt;
        EndedAt = endedAt;
    }

    public Guid CampaignSessionId { get; }
    public Guid CampaignId { get; }
    public DateTime SessionStart { get; }
    public DateTime SessionEnd { get; }
    public string Status { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime? EndedAt { get; private set; }

    public static CampaignSession Create(
        Guid campaignId,
        DateTime sessionStart,
        DateTime sessionEnd,
        DateTime now)
    {
        if (campaignId == Guid.Empty)
        {
            throw new DomainException("CAMPAIGN_ID_REQUIRED", DomainErrorType.Validation);
        }

        if (sessionEnd <= sessionStart)
        {
            throw new DomainException("CAMPAIGN_DATE_RANGE_INVALID", DomainErrorType.Validation);
        }

        return new CampaignSession(
            Guid.NewGuid(),
            campaignId,
            sessionStart,
            sessionEnd,
            CampaignSessionStatuses.Scheduled,
            now,
            null);
    }

    public static CampaignSession Restore(
        Guid campaignSessionId,
        Guid campaignId,
        DateTime sessionStart,
        DateTime sessionEnd,
        string status,
        DateTime createdAt,
        DateTime? endedAt)
    {
        if (campaignSessionId == Guid.Empty)
        {
            throw new DomainException("CAMPAIGN_SESSION_ID_REQUIRED", DomainErrorType.Validation);
        }

        return new CampaignSession(
            campaignSessionId,
            campaignId,
            sessionStart,
            sessionEnd,
            status,
            createdAt,
            endedAt);
    }

    public void Cancel(DateTime operationTimeUtc)
    {
        if (Status.Equals(CampaignSessionStatuses.Scheduled, StringComparison.Ordinal))
        {
            Status = CampaignSessionStatuses.Cancelled;
            return;
        }

        if (Status.Equals(CampaignSessionStatuses.Running, StringComparison.Ordinal))
        {
            Status = CampaignSessionStatuses.Cancelled;
            EndedAt = operationTimeUtc;
            return;
        }

        throw new DomainException(
            "CAMPAIGN_SESSION_NOT_CANCELLABLE",
            DomainErrorType.Conflict);
    }
}
