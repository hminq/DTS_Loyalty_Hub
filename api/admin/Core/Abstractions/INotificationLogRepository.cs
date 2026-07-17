using Core.UseCases.Common;
using Core.UseCases.Notifications.Results;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Abstractions;

public interface INotificationLogRepository
{
    Task<PagedResult<NotificationLogResult>> GetPagedAsync(
        int page, 
        int pageSize, 
        Guid? customerId = null, 
        string? eventTypeCode = null, 
        CancellationToken ct = default);
}
