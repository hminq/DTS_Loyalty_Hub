using System.Text;
using Campaign.Contracts.Actions;
using Campaign.Contracts.Conditions;
using Campaign.Contracts.Constants;
using Campaign.Contracts.Definitions;
using Consumer.Core.Abstractions;
using Consumer.Core.Entities.Campaigns;
using Consumer.Core.Entities.Constants;
using Consumer.Core.Exceptions;
using Consumer.Core.Requests;
using Consumer.Core.Services;
using MediatR;

namespace Consumer.Core.Handlers;

public sealed class ProcessCampaignCommandHandler
    : IRequestHandler<
        ProcessCampaignCommand,
        ProcessCampaignResult>
{
    private readonly ICampaignRewardExecutionStore _store;
    private readonly IVersionedEnvelopeParser _envelopeParser;
    private readonly IEventDefinitionProvider _definitionProvider;
    private readonly GenericCampaignEventFactory _eventFactory;
    private readonly GenericCampaignTargetResolver _targetResolver;
    private readonly ICampaignActionExecutorRegistry _actionExecutorRegistry;
    private readonly CampaignActionCatalog _actionCatalog;
    private readonly GenericCampaignConditionEvaluator _conditionEvaluator;
    private readonly CampaignActionBindingParser _actionBindingParser;
    private readonly TimeProvider _timeProvider;

    public ProcessCampaignCommandHandler(
        ICampaignRewardExecutionStore store,
        IVersionedEnvelopeParser envelopeParser,
        IEventDefinitionProvider definitionProvider,
        GenericCampaignEventFactory eventFactory,
        GenericCampaignTargetResolver targetResolver,
        ICampaignActionExecutorRegistry actionExecutorRegistry,
        CampaignActionCatalog actionCatalog,
        GenericCampaignConditionEvaluator conditionEvaluator,
        CampaignActionBindingParser actionBindingParser,
        TimeProvider timeProvider)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _envelopeParser = envelopeParser ??
            throw new ArgumentNullException(nameof(envelopeParser));
        _definitionProvider = definitionProvider ??
            throw new ArgumentNullException(nameof(definitionProvider));
        _eventFactory = eventFactory ??
            throw new ArgumentNullException(nameof(eventFactory));
        _targetResolver = targetResolver ??
            throw new ArgumentNullException(nameof(targetResolver));
        _actionExecutorRegistry = actionExecutorRegistry ??
            throw new ArgumentNullException(nameof(actionExecutorRegistry));
        _actionCatalog = actionCatalog ??
            throw new ArgumentNullException(nameof(actionCatalog));
        _conditionEvaluator = conditionEvaluator ??
            throw new ArgumentNullException(nameof(conditionEvaluator));
        _actionBindingParser = actionBindingParser ??
            throw new ArgumentNullException(nameof(actionBindingParser));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task<ProcessCampaignResult> Handle(
        ProcessCampaignCommand request,
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

        var campaignEvent = await ValidatePersistedEventAsync(
            context,
            cancellationToken);
        EnsurePinnedConfiguration(context, campaignEvent);

        var operationTime = _timeProvider.GetUtcNow().UtcDateTime;
        var eventDefinition = _eventFactory.ToCampaignDefinition(
            campaignEvent.Definition);

        var conditionResult = _conditionEvaluator.Evaluate(
            context.CampaignConditionJson,
            campaignEvent.Definition,
            campaignEvent.PayloadValues);
        if (!conditionResult.IsValid || conditionResult.Condition is null)
        {
            throw InvalidConfiguration();
        }

        if (!conditionResult.IsMatch)
        {
            return Skip(
                context,
                CampaignProcessingOutcomeCodes.ConditionNotMatched,
                operationTime);
        }

        var actions = await _store.LockActionsAsync(
            context.CampaignId,
            cancellationToken);

        if (actions.Count == 0)
        {
            throw InvalidConfiguration();
        }

        var executableActions = new List<ResolvedCampaignAction>(actions.Count);
        foreach (var action in actions)
        {
            if (!_actionCatalog.TryGetAction(
                    action.ActionType,
                    out var actionDefinition))
            {
                throw InvalidActionConfiguration();
            }

            var parseResult = _actionBindingParser.Parse(
                action.ActionType,
                action.ActionConfigJson,
                eventDefinition,
                actionDefinition,
                trimSelector: false);
            if (!parseResult.IsValid ||
                parseResult.Binding is null ||
                parseResult.Parameters is null)
            {
                throw InvalidActionConfiguration();
            }

            var resolution = _targetResolver.Resolve(
                campaignEvent,
                parseResult.Binding.Target.Selector,
                actionDefinition.RequiredTargetKind);

            if (resolution.Status == CampaignTargetResolutionStatuses.InvalidEvent)
            {
                throw new CampaignEventValidationException(
                    resolution.ErrorCode ??
                    CampaignProcessingErrorCodes.EventPayloadInvalid);
            }

            if (resolution.Status != CampaignTargetResolutionStatuses.Resolved ||
                !resolution.TargetId.HasValue ||
                !string.Equals(
                    resolution.TargetKind,
                    actionDefinition.RequiredTargetKind,
                    StringComparison.Ordinal))
            {
                throw InvalidActionConfiguration();
            }

            executableActions.Add(
                new ResolvedCampaignAction(
                    action.ActionId,
                    actionDefinition.Code,
                    action.ExecuteOrder,
                    parseResult.Binding.Target.Selector,
                    resolution.TargetKind,
                    resolution.TargetId.Value,
                    parseResult.Parameters,
                    action.TotalCount,
                    action.SessionCount,
                    action.UsedCount));
        }

        var targetCustomerIds = executableActions
            .Select(action => action.TargetId)
            .Distinct()
            .Order()
            .ToArray();

        var lockedCustomers = await _store.LockCustomersAsync(
            targetCustomerIds,
            cancellationToken);

        foreach (var targetCustomerId in targetCustomerIds)
        {
            if (lockedCustomers.Contains(targetCustomerId))
            {
                continue;
            }

            throw new CampaignProcessingException(
                CampaignProcessingErrorCodes.CampaignTargetCustomerNotFound,
                retriable: false);
        }

        foreach (var targetCustomerId in targetCustomerIds)
        {
            var usageCounts = await _store.GetCampaignUsageCountsAsync(
                context.CampaignId,
                context.CampaignSessionId,
                targetCustomerId,
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

        var executorGroups = executableActions
            .GroupBy(action => action.ActionType, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .ToArray();
        foreach (var executorGroup in executorGroups)
        {
            await _actionExecutorRegistry
                .GetRequired(executorGroup.Key)
                .PrepareAsync(executorGroup.ToArray(), cancellationToken);
        }

        var actionUsage = await _store.LockActionUsagesAsync(
            executableActions.Select(action => action.ActionId).ToArray(),
            context.CampaignSessionId,
            cancellationToken);

        foreach (var executableAction in executableActions)
        {
            if (executableAction.TotalCount.HasValue &&
                executableAction.UsedCount >= executableAction.TotalCount.Value)
            {
                return Skip(
                    context,
                    CampaignProcessingOutcomeCodes.ActionTotalLimitReached,
                    operationTime);
            }

            var sessionUsed = actionUsage.GetValueOrDefault(executableAction.ActionId);
            if (executableAction.SessionCount.HasValue &&
                sessionUsed >= executableAction.SessionCount.Value)
            {
                return Skip(
                    context,
                    CampaignProcessingOutcomeCodes.ActionSessionLimitReached,
                    operationTime);
            }
        }

        var executionContext = new CampaignActionExecutionContext(
            context.EventCampaignProcessingId,
            context.EventId,
            context.CampaignId,
            context.CampaignSessionId,
            operationTime);
        foreach (var executableAction in executableActions
                     .OrderBy(action => action.ExecuteOrder)
                     .ThenBy(action => action.ActionId))
        {
            _actionExecutorRegistry
                .GetRequired(executableAction.ActionType)
                .Execute(executableAction, executionContext);
            _store.RecordSuccessfulActionExecution(
                new SuccessfulActionExecutionMutation(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    context.EventCampaignProcessingId,
                    context.CampaignId,
                    context.CampaignSessionId,
                    executableAction.TargetId,
                    executableAction.ActionId,
                    operationTime));
        }

        _store.MarkCompleted(context.EventCampaignProcessingId, operationTime);

        return new ProcessCampaignResult(
            context.EventCampaignProcessingId,
            EventCampaignProcessingStatuses.Completed,
            null,
            context.AttemptCount + 1,
            executableActions.Count);
    }

    private async Task<GenericValidatedCampaignEvent> ValidatePersistedEventAsync(
        CampaignRewardProcessingContext context,
        CancellationToken cancellationToken)
    {
        GenericValidatedCampaignEvent campaignEvent;
        try
        {
            var envelope = _envelopeParser.Parse(
                Encoding.UTF8.GetBytes(context.NormalizedPayload),
                context.EventId.ToString("D"),
                context.EventType);
            var definition = await _definitionProvider.GetDefinitionAsync(
                envelope.EventType,
                envelope.EventVersion,
                cancellationToken)
                ?? throw InvalidConfiguration();
            campaignEvent = _eventFactory.Create(
                envelope,
                definition,
                context.RoutingKey);
        }
        catch (EventEnvelopeParseException exception)
        {
            throw PersistedEventCorruption(exception);
        }
        catch (CampaignEventValidationException exception)
        {
            throw PersistedEventCorruption(exception);
        }

        if (campaignEvent.EventId != context.EventId ||
            !string.Equals(campaignEvent.EventType, context.EventType, StringComparison.Ordinal) ||
            campaignEvent.EventVersion != context.EventVersion ||
            !string.Equals(campaignEvent.RoutingKey, context.RoutingKey, StringComparison.Ordinal) ||
            campaignEvent.OccurredAt != context.OccurredAt ||
            !string.Equals(campaignEvent.PayloadHash, context.PayloadHash, StringComparison.Ordinal))
        {
            throw PersistedEventCorruption();
        }

        return campaignEvent;
    }

    private static void EnsurePinnedConfiguration(
        CampaignRewardProcessingContext context,
        GenericValidatedCampaignEvent campaignEvent)
    {
        if (context.EventId != campaignEvent.EventId ||
            context.EventTypeVersionId != campaignEvent.EventTypeVersionId ||
            context.EventVersion != campaignEvent.EventVersion ||
            context.CampaignEventTypeVersionId != campaignEvent.EventTypeVersionId ||
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
            throw InvalidConfiguration();
        }
    }

    private ProcessCampaignResult Skip(
        CampaignRewardProcessingContext context,
        string outcomeCode,
        DateTime operationTime)
    {
        _store.MarkSkipped(
            context.EventCampaignProcessingId,
            outcomeCode,
            operationTime);

        return new ProcessCampaignResult(
            context.EventCampaignProcessingId,
            EventCampaignProcessingStatuses.Skipped,
            outcomeCode,
            context.AttemptCount + 1,
            0);
    }

    private static ProcessCampaignResult ExistingResult(
        CampaignRewardProcessingContext context)
    {
        return new ProcessCampaignResult(
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

    private static CampaignConfigurationException InvalidActionConfiguration()
    {
        return new CampaignConfigurationException(
            CampaignProcessingErrorCodes.CampaignActionConfigurationInvalid);
    }

    private static CampaignProcessingException PersistenceFailure()
    {
        return new CampaignProcessingException(
            CampaignProcessingErrorCodes.CampaignProcessingPersistenceFailed,
            retriable: true);
    }

    private static CampaignProcessingException PersistedEventCorruption(
        Exception? innerException = null)
    {
        return new CampaignProcessingException(
            CampaignProcessingErrorCodes.CampaignProcessingPersistenceFailed,
            retriable: false,
            innerException);
    }
}
