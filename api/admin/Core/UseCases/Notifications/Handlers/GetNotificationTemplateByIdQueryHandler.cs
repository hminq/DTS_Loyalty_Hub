using Core.Abstractions;
using Core.UseCases.Notifications.Queries;
using Core.UseCases.Notifications.Results;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Core.UseCases.Notifications.Handlers;

public sealed class GetNotificationTemplateByIdQueryHandler : IRequestHandler<GetNotificationTemplateByIdQuery, NotificationTemplateResult>
{
    private readonly INotificationTemplateRepository _repository;

    public GetNotificationTemplateByIdQueryHandler(INotificationTemplateRepository repository)
    {
        _repository = repository;
    }

    public async Task<NotificationTemplateResult> Handle(GetNotificationTemplateByIdQuery request, CancellationToken ct)
    {
        var template = await _repository.GetByIdAsync(request.TemplateId, ct);
        if (template == null)
        {
            throw new Core.Exceptions.DomainException(
                "TEMPLATE_NOT_FOUND",
                Core.Exceptions.DomainErrorType.NotFound);
        }
        return template;
    }
}
