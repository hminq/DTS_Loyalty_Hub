using System.Threading;
using System.Threading.Tasks;

namespace Core.Abstractions;

public interface INotificationTemplateQueryService
{
    Task<(string TitleTemplate, string BodyTemplate, string Channel, string AvailableVariablesJson)?> GetActiveTemplateAsync(string eventTypeCode, CancellationToken ct);
}
