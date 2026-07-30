using Core.Abstractions;
using Core.Entities.Constants;
using Core.UseCases.Events.Models;
using Microsoft.EntityFrameworkCore;
using Persistence.Models.Context;

namespace Infrastructure.Implementations;

public sealed class PublishedEventVersionRepository : IPublishedEventVersionRepository
{
    private readonly LoyaltyHubDbContext _dbContext;

    public PublishedEventVersionRepository(LoyaltyHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<PublishedEventVersionReference?> GetPublishedAsync(
        string eventType,
        int eventVersion,
        CancellationToken cancellationToken)
    {
        return _dbContext.EventTypeVersions
            .AsNoTracking()
            .Join(
                _dbContext.EventTypes.AsNoTracking(),
                version => version.EventTypeId,
                type => type.EventTypeId,
                (version, type) => new { Version = version, Type = type })
            .Where(row =>
                row.Type.Code == eventType &&
                row.Version.Version == eventVersion &&
                row.Type.Status == CustomerEventPublicationStatuses.EventTypeActive &&
                row.Version.Status == CustomerEventPublicationStatuses.EventVersionPublished &&
                row.Version.PublishedAt != null)
            .Select(row => new PublishedEventVersionReference(
                row.Version.EventTypeVersionId,
                row.Type.Code,
                row.Version.Version,
                row.Type.RoutingKey))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
