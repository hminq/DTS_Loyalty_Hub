using Core.Abstractions;
using Core.Entities.Campaigns;
using Core.Requests;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Implementations;

public sealed class CampaignProcessingScopeExecutor
    : ICampaignProcessingScopeExecutor
{
    private readonly IServiceScopeFactory _scopeFactory;

    public CampaignProcessingScopeExecutor(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory ??
            throw new ArgumentNullException(nameof(scopeFactory));
    }

    public async Task<PrepareCustomerAccountRegisteredEventResult> PrepareEventAsync(
        ValidatedCustomerAccountRegisteredEvent campaignEvent,
        CancellationToken cancellationToken = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        return await sender.Send(
            new PrepareCustomerAccountRegisteredEventCommand(campaignEvent),
            cancellationToken);
    }

    public async Task<ProcessCustomerRegistrationCampaignResult> ProcessCampaignAsync(
        Guid eventCampaignProcessingId,
        CancellationToken cancellationToken = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        return await sender.Send(
            new ProcessCustomerRegistrationCampaignCommand(
                eventCampaignProcessingId),
            cancellationToken);
    }

    public async Task<RecordCampaignProcessingFailureResult> RecordFailureAsync(
        Guid eventCampaignProcessingId,
        string outcomeCode,
        string errorSummary,
        CancellationToken cancellationToken = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        return await sender.Send(
            new RecordCampaignProcessingFailureCommand(
                eventCampaignProcessingId,
                outcomeCode,
                errorSummary),
            cancellationToken);
    }

    public async Task<FinalizeEventProcessingResult> FinalizeEventAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        return await sender.Send(
            new FinalizeEventProcessingCommand(eventId),
            cancellationToken);
    }
}
