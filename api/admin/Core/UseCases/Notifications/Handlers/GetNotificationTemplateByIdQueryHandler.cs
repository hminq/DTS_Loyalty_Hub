using Core.Abstractions;
using Core.UseCases.Notifications.Queries;
using Core.UseCases.Notifications.Results;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Core.UseCases.Notifications;

public sealed class GetNotificationTemplateByIdQueryHandler : IRequestHandler<GetNotificationTemplateByIdQuery, NotificationTemplateResult?>
{
    private readonly INotificationTemplateRepository _repository;

    public GetNotificationTemplateByIdQueryHandler(INotificationTemplateRepository repository)
    {
        _repository = repository;
    }

    public Task<NotificationTemplateResult?> Handle(GetNotificationTemplateByIdQuery request, CancellationToken ct)
    {
        return _repository.GetByIdAsync(request.TemplateId, ct);
    }
}
