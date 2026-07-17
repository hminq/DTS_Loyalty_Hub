using Core.UseCases.Notifications.Results;

namespace Core.Abstractions;

public interface INotificationEventTypeRepository
{
    Task<IReadOnlyCollection<NotificationEventTypeResult>> GetListAsync(string? keyword = null, CancellationToken ct = default);
    Task<NotificationEventTypeResult?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
}
