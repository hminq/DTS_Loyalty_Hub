namespace Messaging.Contracts.Events;

public sealed record CustomerMissionCompletedPayload(
    Guid CustomerId,
    string MissionCode,
    string MissionCategory,
    bool IsFirstCompletion,
    int CompletionCount);
