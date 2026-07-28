namespace Core.Entities.Campaigns;

public sealed record EventProcessingFinalizationState(
    Guid EventId,
    string Status,
    IReadOnlyList<string> ChildStatuses);
