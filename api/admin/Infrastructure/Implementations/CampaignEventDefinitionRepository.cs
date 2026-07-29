using Core.Abstractions;
using Core.UseCases.Campaigns.Results;
using Microsoft.EntityFrameworkCore;
using Persistence.Models.Context;

namespace Infrastructure.Implementations;

public sealed class CampaignEventDefinitionRepository : ICampaignEventDefinitionRepository
{
    private const string Active = "ACTIVE";
    private const string Published = "PUBLISHED";

    private readonly LoyaltyHubDbContext _dbContext;

    public CampaignEventDefinitionRepository(LoyaltyHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<CampaignEventDefinitionResult?> GetByVersionIdAsync(
        Guid eventTypeVersionId,
        CancellationToken ct = default)
    {
        return ProjectVersions()
            .SingleOrDefaultAsync(item => item.EventTypeVersionId == eventTypeVersionId, ct);
    }

    public async Task<CampaignEventDefinitionResult?> GetForCampaignWriteAsync(
        Guid eventTypeVersionId,
        CancellationToken ct = default)
    {
        var owner = await _dbContext.EventTypeVersions
            .AsNoTracking()
            .Where(version => version.EventTypeVersionId == eventTypeVersionId)
            .Select(version => new
            {
                version.EventTypeId
            })
            .SingleOrDefaultAsync(ct);

        if (owner is null)
        {
            return null;
        }

        _ = await _dbContext.EventTypes
            .FromSqlInterpolated($"SELECT * FROM event_types WHERE event_type_id = {owner.EventTypeId} FOR SHARE")
            .AsNoTracking()
            .SingleAsync(ct);

        return await ProjectVersions()
            .SingleOrDefaultAsync(item => item.EventTypeVersionId == eventTypeVersionId, ct);
    }

    public async Task<IReadOnlyCollection<CampaignEventDefinitionResult>> GetSelectableVersionsAsync(
        CancellationToken ct = default)
    {
        return await ProjectVersions()
            .Where(item =>
                item.EventTypeStatus == Active &&
                item.VersionStatus == Published)
            .OrderBy(item => item.Name)
            .ThenBy(item => item.Code)
            .ThenByDescending(item => item.Version)
            .ToArrayAsync(ct);
    }

    private IQueryable<CampaignEventDefinitionResult> ProjectVersions()
    {
        return _dbContext.EventTypeVersions
            .AsNoTracking()
            .Select(version => new CampaignEventDefinitionResult(
                version.EventType.EventTypeId,
                version.EventTypeVersionId,
                version.EventType.Code,
                version.EventType.RoutingKey,
                version.EventType.Name,
                version.EventType.Status,
                version.Version,
                version.Status,
                version.PayloadSchema));
    }
}
