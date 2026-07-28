using Core.Abstractions;
using Core.Entities.Campaigns;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.Requests;

namespace Core.Services;

public sealed class CampaignEventProcessingCoordinator
{
    private readonly ICampaignProcessingScopeExecutor _scopeExecutor;

    public CampaignEventProcessingCoordinator(
        ICampaignProcessingScopeExecutor scopeExecutor)
    {
        _scopeExecutor = scopeExecutor ??
            throw new ArgumentNullException(nameof(scopeExecutor));
    }

    public async Task<CampaignProcessingSequenceResult> ProcessAsync(
        Guid eventId,
        IReadOnlyCollection<PreparedCampaignTarget> targets,
        CancellationToken cancellationToken = default)
    {
        if (eventId == Guid.Empty)
        {
            throw new ArgumentException("Event ID is required.", nameof(eventId));
        }

        ArgumentNullException.ThrowIfNull(targets);

        var campaignResults =
            new List<ProcessCustomerRegistrationCampaignResult>(targets.Count);
        var failureResults =
            new List<RecordCampaignProcessingFailureResult>();

        foreach (var target in targets
                     .OrderBy(item => item.CampaignId)
                     .ThenBy(item => item.CampaignSessionId))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (target.Status != EventCampaignProcessingStatuses.Pending)
            {
                continue;
            }

            try
            {
                campaignResults.Add(
                    await _scopeExecutor.ProcessCampaignAsync(
                        target.EventCampaignProcessingId,
                        cancellationToken));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                var failure = Classify(exception);
                failureResults.Add(
                    await _scopeExecutor.RecordFailureAsync(
                        target.EventCampaignProcessingId,
                        failure.OutcomeCode,
                        failure.ErrorSummary,
                        cancellationToken));
            }
        }

        var finalization = await _scopeExecutor.FinalizeEventAsync(
            eventId,
            cancellationToken);

        return new CampaignProcessingSequenceResult(
            eventId,
            campaignResults,
            failureResults,
            finalization);
    }

    private static ClassifiedCampaignFailure Classify(Exception exception)
    {
        return exception switch
        {
            CampaignProcessingException processingException =>
                new ClassifiedCampaignFailure(
                    processingException.ErrorCode,
                    processingException.ErrorCode),
            CampaignConfigurationException configurationException =>
                new ClassifiedCampaignFailure(
                    configurationException.ErrorCode,
                    configurationException.ErrorCode),
            CampaignEventValidationException validationException =>
                new ClassifiedCampaignFailure(
                    validationException.ErrorCode,
                    validationException.ErrorCode),
            _ => new ClassifiedCampaignFailure(
                CampaignProcessingErrorCodes.CampaignProcessingUnexpectedError,
                "Unexpected campaign processing failure.")
        };
    }

    private sealed record ClassifiedCampaignFailure(
        string OutcomeCode,
        string ErrorSummary);
}
