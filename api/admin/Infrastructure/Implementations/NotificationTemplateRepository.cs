using Core.Abstractions;
using Core.UseCases.Common;
using Core.UseCases.Notifications.Results;
using Microsoft.EntityFrameworkCore;
using Persistence.Models.Context;
using DomainNotificationTemplate = Core.Entities.NotificationTemplate;
using PersistenceNotificationTemplate = Persistence.Models.NotificationTemplate;

namespace Infrastructure.Implementations;

public sealed class NotificationTemplateRepository : INotificationTemplateRepository
{
    private readonly LoyaltyHubDbContext _dbContext;

    public NotificationTemplateRepository(LoyaltyHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<NotificationTemplateResult>> GetPagedAsync(
        int page, int pageSize,
        string? keyword = null, string? eventTypeCode = null, string? channel = null, string? language = null, bool? isActive = null,
        CancellationToken ct = default)
    {
        var query = _dbContext.NotificationTemplates
            .Include(t => t.NotificationEventType)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var lowerKeyword = keyword.ToLower();
            query = query.Where(t => t.Name.ToLower().Contains(lowerKeyword) || 
                                     t.TitleTemplate.ToLower().Contains(lowerKeyword) || 
                                     t.BodyTemplate.ToLower().Contains(lowerKeyword));
        }

        if (!string.IsNullOrWhiteSpace(eventTypeCode))
        {
            query = query.Where(t => t.NotificationEventType.EventTypeCode == eventTypeCode);
        }

        if (!string.IsNullOrWhiteSpace(channel))
        {
            query = query.Where(t => t.Channel == channel);
        }

        if (!string.IsNullOrWhiteSpace(language))
        {
            query = query.Where(t => t.Language == language);
        }

        if (isActive.HasValue)
        {
            query = query.Where(t => t.IsActive == isActive.Value);
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new NotificationTemplateResult(
                x.TemplateId,
                x.NotificationEventTypeId,
                x.NotificationEventType.EventTypeCode,
                x.NotificationEventType.DisplayName,
                x.Channel,
                x.Language,
                x.Name,
                x.TitleTemplate,
                x.BodyTemplate,
                x.IsActive,
                x.CreatedBy,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(ct);

        return new PagedResult<NotificationTemplateResult>(items, page, pageSize, total);
    }

    public async Task<NotificationTemplateResult?> GetByIdAsync(Guid templateId, CancellationToken ct = default)
    {
        return await _dbContext.NotificationTemplates
            .AsNoTracking()
            .Where(t => t.TemplateId == templateId)
            .Select(x => new NotificationTemplateResult(
                x.TemplateId,
                x.NotificationEventTypeId,
                x.NotificationEventType.EventTypeCode,
                x.NotificationEventType.DisplayName,
                x.Channel,
                x.Language,
                x.Name,
                x.TitleTemplate,
                x.BodyTemplate,
                x.IsActive,
                x.CreatedBy,
                x.CreatedAt,
                x.UpdatedAt))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<DomainNotificationTemplate?> GetEntityByIdAsync(Guid templateId, CancellationToken ct = default)
    {
        var model = await _dbContext.NotificationTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TemplateId == templateId, ct);
            
        if (model == null) return null;

        return DomainNotificationTemplate.Restore(
            model.TemplateId,
            model.NotificationEventTypeId,
            model.Channel,
            model.Language,
            model.Name,
            model.TitleTemplate,
            model.BodyTemplate,
            model.IsActive,
            model.CreatedBy,
            model.CreatedAt,
            model.UpdatedAt);
    }

    public DomainNotificationTemplate Add(DomainNotificationTemplate template)
    {
        _dbContext.NotificationTemplates.Add(new PersistenceNotificationTemplate
        {
            TemplateId = template.TemplateId,
            NotificationEventTypeId = template.NotificationEventTypeId,
            Channel = template.Channel,
            Language = template.Language,
            Name = template.Name,
            TitleTemplate = template.TitleTemplate,
            BodyTemplate = template.BodyTemplate,
            IsActive = template.IsActive,
            CreatedBy = template.CreatedBy,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt
        });

        return template;
    }

    public async Task UpdateAsync(DomainNotificationTemplate template, CancellationToken ct = default)
    {
        var persistedTemplate = await _dbContext.NotificationTemplates
            .FirstOrDefaultAsync(t => t.TemplateId == template.TemplateId, ct);

        if (persistedTemplate == null)
        {
            throw new Core.Exceptions.DomainException(
                "TEMPLATE_NOT_FOUND",
                Core.Exceptions.DomainErrorType.NotFound);
        }

        persistedTemplate.NotificationEventTypeId = template.NotificationEventTypeId;
        persistedTemplate.Channel = template.Channel;
        persistedTemplate.Language = template.Language;
        persistedTemplate.Name = template.Name;
        persistedTemplate.TitleTemplate = template.TitleTemplate;
        persistedTemplate.BodyTemplate = template.BodyTemplate;
        persistedTemplate.IsActive = template.IsActive;
        persistedTemplate.UpdatedAt = template.UpdatedAt;
    }

    public async Task DeactivateOtherTemplatesAsync(Guid excludeTemplateId, Guid notificationEventTypeId, string channel, string language, CancellationToken ct = default)
    {
        var otherActiveTemplates = await _dbContext.NotificationTemplates
            .Where(t => t.NotificationEventTypeId == notificationEventTypeId &&
                        t.Channel == channel &&
                        t.Language == language &&
                        t.IsActive &&
                        t.TemplateId != excludeTemplateId)
            .ToListAsync(ct);

        if (otherActiveTemplates.Any())
        {
            foreach (var t in otherActiveTemplates)
            {
                t.IsActive = false;
                t.UpdatedAt = DateTime.UtcNow;
            }
            await _dbContext.SaveChangesAsync(ct);
        }
    }
}
