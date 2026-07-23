using System.Threading;
using System.Threading.Tasks;
using Core.Abstractions;
using Core.Exceptions;
using Core.UseCases.Notifications.Results;
using MediatR;

namespace Core.UseCases.Notifications.Commands.Simulate;

public sealed class SimulateNotificationCommandHandler : IRequestHandler<SimulateNotificationCommand, SimulateNotificationResult>
{
    private readonly INotificationTemplateQueryService _templateQueryService;
    private readonly ICustomerRepository _customerRepository;
    private readonly ITemplateEngine _templateEngine;

    public SimulateNotificationCommandHandler(
        INotificationTemplateQueryService templateQueryService,
        ICustomerRepository customerRepository,
        ITemplateEngine templateEngine)
    {
        _templateQueryService = templateQueryService;
        _customerRepository = customerRepository;
        _templateEngine = templateEngine;
    }

    public async Task<SimulateNotificationResult> Handle(SimulateNotificationCommand request, CancellationToken ct)
    {
        var template = await _templateQueryService.GetActiveTemplateAsync(request.EventTypeCode, ct);
        
        if (template == null)
        {
            throw new DomainException("NO_ACTIVE_TEMPLATE", DomainErrorType.NotFound, $"No active template found for event {request.EventTypeCode}");
        }

        var customer = await _customerRepository.GetCustomerWithTiersAsync(request.CustomerId, ct);

        // Build data context with real DB data.
        // Property paths must match the AvailableVariables JSON config (e.g. "custInfo.Name", "currentTier.Name")
        var dataContext = new
        {
            custInfo = new { Name = customer?.FullName ?? "" },
            currentTier = new { Name = customer?.CurrentTierName ?? "", Points = customer?.CurrentTierPoint.ToString("N0") ?? "0" },
            nextTier = new { Name = customer?.NextTierName ?? "", Points = customer?.NextTierPoint.ToString("N0") ?? "0" }
        };

        var title = _templateEngine.Render(template.Value.TitleTemplate, template.Value.AvailableVariablesJson, dataContext);
        var body = _templateEngine.Render(template.Value.BodyTemplate, template.Value.AvailableVariablesJson, dataContext);

        return new SimulateNotificationResult(
            Title: title,
            Body: body,
            Channel: template.Value.Channel
        );
    }
}
