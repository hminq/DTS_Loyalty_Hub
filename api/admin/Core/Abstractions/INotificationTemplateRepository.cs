using Core.Entities;
using Core.UseCases.Notifications.Results;

using Core.UseCases.Common;

namespace Core.Abstractions;

public interface INotificationTemplateRepository
{
    Task<PagedResult<NotificationTemplateResult>> GetPagedAsync(
        int page, int pageSize,
        string? keyword = null, string? eventTypeCode = null, string? channel = null, string? language = null, bool? isActive = null,
        CancellationToken ct = default);
    Task<NotificationTemplateResult?> GetByIdAsync(Guid templateId, CancellationToken ct = default);
    Task<NotificationTemplate?> GetEntityByIdAsync(Guid templateId, CancellationToken ct = default);
    NotificationTemplate Add(NotificationTemplate template);
    Task UpdateAsync(NotificationTemplate template, CancellationToken ct = default);
}
