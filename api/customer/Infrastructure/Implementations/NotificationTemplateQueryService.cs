using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Abstractions;
using Microsoft.EntityFrameworkCore;
using Persistence.Models.Context;

namespace Infrastructure.Implementations;

public sealed class NotificationTemplateQueryService : INotificationTemplateQueryService
{
    private readonly LoyaltyHubDbContext _dbContext;

    public NotificationTemplateQueryService(LoyaltyHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(string TitleTemplate, string BodyTemplate, string Channel, string AvailableVariablesJson)?> GetActiveTemplateAsync(string eventTypeCode, CancellationToken ct)
    {
        var template = await _dbContext.NotificationTemplates
            .Include(t => t.NotificationEventType)
            .Where(t => t.NotificationEventType.EventTypeCode == eventTypeCode && t.IsActive)
            .Select(t => new
            {
                t.TitleTemplate,
                t.BodyTemplate,
                t.Channel,
                t.NotificationEventType.AvailableVariables
            })
            .FirstOrDefaultAsync(ct);

        if (template == null) return null;

        return (template.TitleTemplate, template.BodyTemplate, template.Channel, template.AvailableVariables);
    }
}
