using System.Text.Json;
using Core.Abstractions;
using Core.Entities;
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
        int page,
        int pageSize,
        string? keyword = null,
        string? notificationCode = null,
        string? channel = null,
        string? language = null,
        bool? isActive = null,
        CancellationToken ct = default)
    {
        var query = _dbContext.NotificationTemplates.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var lowerKeyword = keyword.ToLower();
            query = query.Where(t =>
                t.Name.ToLower().Contains(lowerKeyword) ||
                t.TitleTemplate.ToLower().Contains(lowerKeyword) ||
                t.BodyTemplate.ToLower().Contains(lowerKeyword));
        }

        if (!string.IsNullOrWhiteSpace(notificationCode))
            query = query.Where(t => t.NotificationCode == notificationCode);
        if (!string.IsNullOrWhiteSpace(channel))
            query = query.Where(t => t.Channel == channel);
        if (!string.IsNullOrWhiteSpace(language))
            query = query.Where(t => t.Language == language);
        if (isActive.HasValue)
            query = query.Where(t => t.IsActive == isActive.Value);

        var total = await query.CountAsync(ct);
        var models = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        var items = models.Select(ToResult).ToArray();

        return new PagedResult<NotificationTemplateResult>(items, page, pageSize, total);
    }

    public async Task<NotificationTemplateResult?> GetByIdAsync(
        Guid templateId,
        CancellationToken ct = default)
    {
        var model = await _dbContext.NotificationTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TemplateId == templateId, ct);

        return model is null ? null : ToResult(model);
    }

    public async Task<DomainNotificationTemplate?> GetEntityByIdAsync(
        Guid templateId,
        CancellationToken ct = default)
    {
        var model = await _dbContext.NotificationTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TemplateId == templateId, ct);

        return model is null ? null : ToDomain(model);
    }

    public DomainNotificationTemplate Add(DomainNotificationTemplate template)
    {
        _dbContext.NotificationTemplates.Add(ToPersistence(template));
        return template;
    }

    public Task<bool> HasActiveTemplateAsync(
        Guid excludeTemplateId,
        string notificationCode,
        string channel,
        string language,
        CancellationToken ct = default)
    {
        return _dbContext.NotificationTemplates.AnyAsync(t =>
            t.TemplateId != excludeTemplateId &&
            t.NotificationCode == notificationCode &&
            t.Channel == channel &&
            t.Language == language &&
            t.IsActive,
            ct);
    }

    public async Task UpdateAsync(
        DomainNotificationTemplate template,
        CancellationToken ct = default)
    {
        var persisted = await _dbContext.NotificationTemplates
            .FirstOrDefaultAsync(t => t.TemplateId == template.TemplateId, ct);

        if (persisted is null)
            throw new Core.Exceptions.DomainException("TEMPLATE_NOT_FOUND", Core.Exceptions.DomainErrorType.NotFound);

        persisted.NotificationCode = template.NotificationCode;
        persisted.Channel = template.Channel;
        persisted.Language = template.Language;
        persisted.Name = template.Name;
        persisted.TitleTemplate = template.TitleTemplate;
        persisted.BodyTemplate = template.BodyTemplate;
        persisted.VariableDefinitions = JsonSerializer.Serialize(template.Variables);
        persisted.IsActive = template.IsActive;
        persisted.UpdatedAt = template.UpdatedAt;
    }

    private static PersistenceNotificationTemplate ToPersistence(DomainNotificationTemplate template) =>
        new()
        {
            TemplateId = template.TemplateId,
            NotificationCode = template.NotificationCode,
            Channel = template.Channel,
            Language = template.Language,
            Name = template.Name,
            TitleTemplate = template.TitleTemplate,
            BodyTemplate = template.BodyTemplate,
            VariableDefinitions = JsonSerializer.Serialize(template.Variables),
            IsActive = template.IsActive,
            CreatedBy = template.CreatedBy,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt
        };

    private static DomainNotificationTemplate ToDomain(PersistenceNotificationTemplate model) =>
        DomainNotificationTemplate.Restore(
            model.TemplateId,
            model.NotificationCode,
            model.Channel,
            model.Language,
            model.Name,
            model.TitleTemplate,
            model.BodyTemplate,
            DeserializeVariables(model.VariableDefinitions),
            model.IsActive,
            model.CreatedBy,
            model.CreatedAt,
            model.UpdatedAt);

    private static NotificationTemplateResult ToResult(PersistenceNotificationTemplate model) =>
        new(
            model.TemplateId,
            model.NotificationCode,
            model.Channel,
            model.Language,
            model.Name,
            model.TitleTemplate,
            model.BodyTemplate,
            DeserializeVariables(model.VariableDefinitions),
            model.IsActive,
            model.CreatedBy,
            model.CreatedAt,
            model.UpdatedAt);

    private static IReadOnlyCollection<NotificationTemplateVariable> DeserializeVariables(string? json) =>
        string.IsNullOrWhiteSpace(json)
            ? Array.Empty<NotificationTemplateVariable>()
            : JsonSerializer.Deserialize<IReadOnlyCollection<NotificationTemplateVariable>>(json)
              ?? Array.Empty<NotificationTemplateVariable>();
}
