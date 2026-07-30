namespace Consumer.Core.Entities.Campaigns;

public sealed record ResolvedCampaignAction(
    Guid ActionId,
    string ActionType,
    int ExecuteOrder,
    string TargetSelector,
    string TargetKind,
    Guid TargetId,
    object ParsedParameters,
    int? TotalCount,
    int? SessionCount,
    int UsedCount);
