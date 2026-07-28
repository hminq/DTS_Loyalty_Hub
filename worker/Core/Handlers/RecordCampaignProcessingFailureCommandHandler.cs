using System.Text;
using Core.Abstractions;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.Requests;
using MediatR;

namespace Core.Handlers;

public sealed class RecordCampaignProcessingFailureCommandHandler
    : IRequestHandler<
        RecordCampaignProcessingFailureCommand,
        RecordCampaignProcessingFailureResult>
{
    internal const int MaximumErrorSummaryLength = 500;

    private readonly IEventProcessingFinalizationStore _store;
    private readonly TimeProvider _timeProvider;

    public RecordCampaignProcessingFailureCommandHandler(
        IEventProcessingFinalizationStore store,
        TimeProvider timeProvider)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task<RecordCampaignProcessingFailureResult> Handle(
        RecordCampaignProcessingFailureCommand request,
        CancellationToken cancellationToken)
    {
        if (request.EventCampaignProcessingId == Guid.Empty ||
            !IsStableOutcomeCode(request.OutcomeCode))
        {
            throw new ArgumentException(
                "A processing ID and stable outcome code are required.",
                nameof(request));
        }

        var state = await _store.LockCampaignProcessingAsync(
            request.EventCampaignProcessingId,
            cancellationToken)
            ?? throw PersistenceFailure();

        if (state.Status != EventCampaignProcessingStatuses.Pending)
        {
            return new RecordCampaignProcessingFailureResult(
                state.EventCampaignProcessingId,
                state.Status,
                state.OutcomeCode,
                state.AttemptCount,
                Updated: false);
        }

        var errorSummary = Sanitize(request.ErrorSummary);
        _store.MarkCampaignFailed(
            state.EventCampaignProcessingId,
            request.OutcomeCode,
            errorSummary,
            _timeProvider.GetUtcNow().UtcDateTime);

        return new RecordCampaignProcessingFailureResult(
            state.EventCampaignProcessingId,
            EventCampaignProcessingStatuses.Failed,
            request.OutcomeCode,
            state.AttemptCount + 1,
            Updated: true);
    }

    internal static string Sanitize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "Campaign processing failed.";
        }

        var builder = new StringBuilder(Math.Min(value.Length, MaximumErrorSummaryLength));
        var previousWasWhitespace = false;

        foreach (var character in value)
        {
            if (char.IsControl(character) || char.IsWhiteSpace(character))
            {
                if (!previousWasWhitespace && builder.Length > 0)
                {
                    builder.Append(' ');
                    previousWasWhitespace = true;
                }

                continue;
            }

            builder.Append(character);
            previousWasWhitespace = false;

            if (builder.Length == MaximumErrorSummaryLength)
            {
                break;
            }
        }

        var sanitized = builder.ToString().Trim();
        return sanitized.Length == 0
            ? "Campaign processing failed."
            : sanitized;
    }

    private static bool IsStableOutcomeCode(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) &&
               value.Length <= 100 &&
               value.All(character =>
                   character is >= 'A' and <= 'Z' ||
                   character is >= '0' and <= '9' ||
                   character == '_');
    }

    private static CampaignProcessingException PersistenceFailure()
    {
        return new CampaignProcessingException(
            CampaignProcessingErrorCodes.CampaignProcessingPersistenceFailed,
            retriable: true);
    }
}
