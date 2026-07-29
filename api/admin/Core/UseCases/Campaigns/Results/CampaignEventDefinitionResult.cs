namespace Core.UseCases.Campaigns.Results;

public sealed record CampaignEventDefinitionResult(
    Guid EventTypeId,
    Guid EventTypeVersionId,
    string Code,
    string RoutingKey,
    string Name,
    string EventTypeStatus,
    int Version,
    string VersionStatus,
    string PayloadSchema);

public sealed record CampaignEventDefinitionReferenceResult(
    Guid EventTypeId,
    Guid EventTypeVersionId,
    string Code,
    string Name,
    int Version);
