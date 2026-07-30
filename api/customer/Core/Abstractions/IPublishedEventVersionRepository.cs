using Core.UseCases.Events.Models;

namespace Core.Abstractions;

public interface IPublishedEventVersionRepository
{
    Task<PublishedEventVersionReference?> GetPublishedAsync(
        string eventType,
        int eventVersion,
        CancellationToken cancellationToken);
}
