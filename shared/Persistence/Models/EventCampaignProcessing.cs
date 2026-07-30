using System;
using System.Collections.Generic;

namespace Persistence.Models;

public partial class EventCampaignProcessing
{
    public Guid EventCampaignProcessingId { get; set; }

    public Guid EventId { get; set; }

    public Guid CampaignId { get; set; }

    public Guid CampaignSessionId { get; set; }

    public string Status { get; set; } = null!;

    public int AttemptCount { get; set; }

    public string? OutcomeCode { get; set; }

    public string? LastError { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public virtual Campaign Campaign { get; set; } = null!;

    public virtual CampaignSession CampaignSession { get; set; } = null!;

    public virtual ICollection<CampaignUsage> CampaignUsages { get; set; } =
        new List<CampaignUsage>();

    public virtual EventProcessing Event { get; set; } = null!;

}
