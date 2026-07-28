using System.Text;
using Campaign.Contracts.Constants;
using Core.Abstractions;
using Core.Entities.Campaigns;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.Requests;
using MediatR;
using Messaging.Contracts.Events;

namespace Core.Handlers;

public sealed class ProcessCustomerRegistrationCampaignCommandHandler
    : IRequestHandler<
        ProcessCustomerRegistrationCampaignCommand,
        ProcessCustomerRegistrationCampaignResult>
{
    private readonly ICampaignRewardExecutionStore _store;
    private readonly ICustomerAccountRegisteredEventValidator _eventValidator;
    private readonly ICustomerAccountRegisteredConditionEvaluator _conditionEvaluator;
    private readonly IIssuePointActionConfigParser _actionConfigParser;
    private readonly TimeProvider _timeProvider;

    public ProcessCustomerRegistrationCampaignCommandHandler(
        ICampaignRewardExecutionStore store,
        ICustomerAccountRegisteredEventValidator eventValidator,
        ICustomerAccountRegisteredConditionEvaluator conditionEvaluator,
        IIssuePointActionConfigParser actionConfigParser,
        TimeProvider timeProvider)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _eventValidator = eventValidator ?? throw new ArgumentNullException(nameof(eventValidator));
        _conditionEvaluator = conditionEvaluator ??
            throw new ArgumentNullException(nameof(conditionEvaluator));
        _actionConfigParser = actionConfigParser ??
            throw new ArgumentNullException(nameof(actionConfigParser));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task<ProcessCustomerRegistrationCampaignResult> Handle(
        ProcessCustomerRegistrationCampaignCommand request,
        CancellationToken cancellationToken)
    {
        if (request.EventCampaignProcessingId == Guid.Empty)
        {
            throw new ArgumentException(
                "Event campaign processing ID is required.",
                nameof(request));
        }

        var context = await _store.LockProcessingAsync(
            request.EventCampaignProcessingId,
            cancellationToken)
            ?? throw PersistenceFailure();

        if (context.Status != EventCampaignProcessingStatuses.Pending)
        {
            return ExistingResult(context);
        }

        if (context.CampaignStatus == CampaignStatuses.Cancelled ||
            context.SessionStatus == CampaignSessionStatuses.Cancelled)
        {
            return Skip(
                context,
                CampaignProcessingOutcomeCodes.CampaignCancelled,
                _timeProvider.GetUtcNow().UtcDateTime);
        }

        EnsureEligibleLifecycle(context);

        var campaignEvent = ValidatePersistedEvent(context);
        EnsurePinnedConfiguration(context, campaignEvent);

        var operationTime = _timeProvider.GetUtcNow().UtcDateTime;
        if (!_conditionEvaluator.IsMatch(
                context.CampaignConditionJson,
                campaignEvent))
        {
            return Skip(
                context,
                CampaignProcessingOutcomeCodes.ConditionNotMatched,
                operationTime);
        }

        var customerIdsToLock = campaignEvent.ReferrerCustomerId.HasValue
            ? new[]
            {
                campaignEvent.CustomerId,
                campaignEvent.ReferrerCustomerId.Value
            }
            : [campaignEvent.CustomerId];
        customerIdsToLock = customerIdsToLock
            .Distinct()
            .Order()
            .ToArray();

        var lockedCustomers = await _store.LockCustomersAsync(
            customerIdsToLock,
            cancellationToken);

        if (!lockedCustomers.Contains(campaignEvent.CustomerId))
        {
            throw new CampaignProcessingException(
                CampaignProcessingErrorCodes.EventCustomerNotFound,
                retriable: false);
        }

        var actions = await _store.LockActionsAsync(
            context.CampaignId,
            cancellationToken);

        if (actions.Count == 0)
        {
            throw InvalidConfiguration();
        }

        var executableActions = actions
            .Select(action =>
            {
                var config = _actionConfigParser.Parse(
                    action.ActionType,
                    action.ActionConfigJson);
                var recipientCustomerId = ResolveRecipient(
                    config.Recipient,
                    campaignEvent);

                return new ExecutableAction(
                    action,
                    recipientCustomerId,
                    config.Amount!.Value);
            })
            .ToArray();

        var recipientCustomerIds = executableActions
            .Select(action => action.RecipientCustomerId)
            .Distinct()
            .Order()
            .ToArray();

        foreach (var recipientCustomerId in recipientCustomerIds)
        {
            if (lockedCustomers.Contains(recipientCustomerId))
            {
                continue;
            }

            var errorCode = recipientCustomerId == campaignEvent.CustomerId
                ? CampaignProcessingErrorCodes.EventCustomerNotFound
                : CampaignProcessingErrorCodes.ReferrerCustomerNotFound;
            throw new CampaignProcessingException(errorCode, retriable: false);
        }

        foreach (var recipientCustomerId in recipientCustomerIds)
        {
            var usageCounts = await _store.GetCampaignUsageCountsAsync(
                context.CampaignId,
                context.CampaignSessionId,
                recipientCustomerId,
                cancellationToken);

            if (context.UserLimitTotal.HasValue &&
                usageCounts.TotalUsed >= context.UserLimitTotal.Value)
            {
                return Skip(
                    context,
                    CampaignProcessingOutcomeCodes.UserTotalLimitReached,
                    operationTime);
            }

            if (context.UserLimitSession.HasValue &&
                usageCounts.SessionUsed >= context.UserLimitSession.Value)
            {
                return Skip(
                    context,
                    CampaignProcessingOutcomeCodes.UserSessionLimitReached,
                    operationTime);
            }
        }

        await _store.LockCustomerPointsAsync(
            recipientCustomerIds,
            cancellationToken);

        var actionUsage = await _store.LockActionUsagesAsync(
            actions.Select(action => action.ActionId).ToArray(),
            context.CampaignSessionId,
            cancellationToken);

        foreach (var executableAction in executableActions)
        {
            var action = executableAction.Action;

            if (action.TotalCount.HasValue &&
                action.UsedCount >= action.TotalCount.Value)
            {
                return Skip(
                    context,
                    CampaignProcessingOutcomeCodes.ActionTotalLimitReached,
                    operationTime);
            }

            var sessionUsed = actionUsage.GetValueOrDefault(action.ActionId);
            if (action.SessionCount.HasValue &&
                sessionUsed >= action.SessionCount.Value)
            {
                return Skip(
                    context,
                    CampaignProcessingOutcomeCodes.ActionSessionLimitReached,
                    operationTime);
            }
        }

        foreach (var executableAction in executableActions)
        {
            _store.ApplyPointReward(
                new PointRewardMutation(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    context.EventCampaignProcessingId,
                    context.EventId,
                    context.CampaignId,
                    context.CampaignSessionId,
                    executableAction.RecipientCustomerId,
                    executableAction.Action.ActionId,
                    executableAction.Amount,
                    operationTime));
        }

        _store.MarkCompleted(context.EventCampaignProcessingId, operationTime);

        return new ProcessCustomerRegistrationCampaignResult(
            context.EventCampaignProcessingId,
            EventCampaignProcessingStatuses.Completed,
            null,
            context.AttemptCount + 1,
            executableActions.Length);
    }

    private ValidatedCustomerAccountRegisteredEvent ValidatePersistedEvent(
        CampaignRewardProcessingContext context)
    {
        var campaignEvent = _eventValidator.Validate(
            Encoding.UTF8.GetBytes(context.NormalizedPayload),
            context.EventId.ToString("D"),
            context.EventType,
            context.RoutingKey);

        if (campaignEvent.PayloadHash != context.PayloadHash)
        {
            throw new CampaignProcessingException(
                CampaignProcessingErrorCodes.CampaignProcessingPersistenceFailed,
                retriable: false);
        }

        return campaignEvent;
    }

    private static void EnsurePinnedConfiguration(
        CampaignRewardProcessingContext context,
        ValidatedCustomerAccountRegisteredEvent campaignEvent)
    {
        if (context.EventId != campaignEvent.EventId ||
            context.EventCustomerId != campaignEvent.CustomerId ||
            context.CampaignEventType != campaignEvent.EventType ||
            context.SessionCampaignId != context.CampaignId ||
            campaignEvent.OccurredAt < context.SessionStart ||
            campaignEvent.OccurredAt >= context.SessionEnd)
        {
            throw InvalidConfiguration();
        }
    }

    private static void EnsureEligibleLifecycle(CampaignRewardProcessingContext context)
    {
        var campaignEligible =
            context.CampaignStatus == CampaignStatuses.Active ||
            context.CampaignStatus == CampaignStatuses.Ended;
        var sessionEligible =
            context.SessionStatus == CampaignSessionStatuses.Scheduled ||
            context.SessionStatus == CampaignSessionStatuses.Running ||
            context.SessionStatus == CampaignSessionStatuses.Ended;

        if (!campaignEligible || !sessionEligible)
        {
            throw PersistenceFailure();
        }
    }

    private static Guid ResolveRecipient(
        string recipient,
        ValidatedCustomerAccountRegisteredEvent campaignEvent)
    {
        if (recipient == PointRecipients.EventCustomer)
        {
            return campaignEvent.CustomerId;
        }

        if (recipient == PointRecipients.Referrer &&
            campaignEvent.Source == CustomerRegistrationSources.Referral &&
            campaignEvent.ReferrerCustomerId.HasValue)
        {
            return campaignEvent.ReferrerCustomerId.Value;
        }

        throw InvalidConfiguration();
    }

    private ProcessCustomerRegistrationCampaignResult Skip(
        CampaignRewardProcessingContext context,
        string outcomeCode,
        DateTime operationTime)
    {
        _store.MarkSkipped(
            context.EventCampaignProcessingId,
            outcomeCode,
            operationTime);

        return new ProcessCustomerRegistrationCampaignResult(
            context.EventCampaignProcessingId,
            EventCampaignProcessingStatuses.Skipped,
            outcomeCode,
            context.AttemptCount + 1,
            0);
    }

    private static ProcessCustomerRegistrationCampaignResult ExistingResult(
        CampaignRewardProcessingContext context)
    {
        return new ProcessCustomerRegistrationCampaignResult(
            context.EventCampaignProcessingId,
            context.Status,
            context.OutcomeCode,
            context.AttemptCount,
            0);
    }

    private static CampaignConfigurationException InvalidConfiguration()
    {
        return new CampaignConfigurationException(
            CampaignProcessingErrorCodes.CampaignConfigurationInvalid);
    }

    private static CampaignProcessingException PersistenceFailure()
    {
        return new CampaignProcessingException(
            CampaignProcessingErrorCodes.CampaignProcessingPersistenceFailed,
            retriable: true);
    }

    private sealed record ExecutableAction(
        CampaignRewardAction Action,
        Guid RecipientCustomerId,
        decimal Amount);
}
