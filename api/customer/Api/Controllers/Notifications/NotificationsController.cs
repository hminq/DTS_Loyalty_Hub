using System.Threading;
using System.Threading.Tasks;
using Api.Authentication;
using Api.Dtos.Requests.Notifications;
using Core.UseCases.Notifications.Commands.Simulate;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Notifications;

[Route("api/[controller]")]
[ApiController]
public sealed class NotificationsController : CustomerControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(ICurrentCustomerAccessor currentCustomerAccessor, IMediator mediator)
        : base(currentCustomerAccessor)
    {
        _mediator = mediator;
    }

    [HttpPost("simulate")]
    public async Task<IActionResult> Simulate([FromBody] SimulateNotificationRequestDto request, CancellationToken ct)
    {
        var command = new SimulateNotificationCommand(CurrentCustomer.CustomerId, request.EventTypeCode);
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }
}
