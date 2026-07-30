using Consumer.Core.Abstractions;
using Consumer.Core.Entities.Campaigns;
using Consumer.Core.Requests;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Consumer.Infrastructure.Implementations;

public sealed class CampaignProcessingScopeExecutor
    : ICampaignProcessingScopeExecutor
{
    private readonly IServiceScopeFactory _scopeFactory;

    public CampaignProcessingScopeExecutor(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory ??
            throw new ArgumentNullException(nameof(scopeFactory));
    }

    public async Task<PrepareCampaignEventResult> PrepareEventAsync(
        IValidatedCampaignEvent campaignEvent,
        CancellationToken cancellationToken = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        return await sender.Send(
            new PrepareCampaignEventCommand(campaignEvent),
            cancellationToken);
    }

    public async Task<ProcessCampaignResult> ProcessCampaignAsync(
        Guid eventCampaignProcessingId,
        CancellationToken cancellationToken = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        return await sender.Send(
            new ProcessCampaignCommand(
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
