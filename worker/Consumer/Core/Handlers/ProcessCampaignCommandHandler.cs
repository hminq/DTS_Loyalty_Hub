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
    private readonly ICampaignActionExecutorRegistry _actionExecutorRegistry;
    private readonly CampaignDefinitionCatalog _definitionCatalog;
    private readonly CampaignConditionParser _conditionParser;
    private readonly CampaignConditionEvaluator _conditionEvaluator;
    private readonly CampaignConditionCompatibilityAnalyzer _compatibilityAnalyzer;
    private readonly CampaignActionBindingParser _actionBindingParser;
    private readonly TimeProvider _timeProvider;

    public ProcessCampaignCommandHandler(
        ICampaignRewardExecutionStore store,
        IVersionedEnvelopeParser envelopeParser,
        IEventDefinitionProvider definitionProvider,
        GenericCampaignEventFactory eventFactory,
        ICampaignActionExecutorRegistry actionExecutorRegistry,
        CampaignDefinitionCatalog definitionCatalog,
        CampaignConditionParser conditionParser,
        CampaignConditionEvaluator conditionEvaluator,
        CampaignConditionCompatibilityAnalyzer compatibilityAnalyzer,
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
        _actionExecutorRegistry = actionExecutorRegistry ??
            throw new ArgumentNullException(nameof(actionExecutorRegistry));
        _definitionCatalog = definitionCatalog ??
            throw new ArgumentNullException(nameof(definitionCatalog));
        _conditionParser = conditionParser ??
            throw new ArgumentNullException(nameof(conditionParser));
        _conditionEvaluator = conditionEvaluator ??
            throw new ArgumentNullException(nameof(conditionEvaluator));
        _compatibilityAnalyzer = compatibilityAnalyzer ??
            throw new ArgumentNullException(nameof(compatibilityAnalyzer));
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

        var conditionResult = _conditionParser.Parse(
            context.CampaignConditionJson,
            eventDefinition);
        if (!conditionResult.IsValid || conditionResult.Condition is null)
        {
            throw InvalidConfiguration();
        }

        var facts = eventDefinition.ConditionFields.ToDictionary(
            field => field.Code,
            field => _eventFactory.GetFact(campaignEvent, field.Code).Value,
            StringComparer.Ordinal);
        if (!_conditionEvaluator.Matches(
                conditionResult.Condition,
                eventDefinition,
                facts))
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
            if (!_definitionCatalog.TryGetAction(
                    action.ActionType,
                    out var actionDefinition))
            {
                throw InvalidActionConfiguration();
            }

            var parseResult = _actionBindingParser.Parse(
                action.ActionType,
                action.ActionConfigJson,
                eventDefinition,
                actionDefinition);
            if (!parseResult.IsValid ||
                parseResult.Binding is null ||
                parseResult.Parameters is null)
            {
                throw InvalidActionConfiguration();
            }

            var targetDefinition = eventDefinition.Targets.Single(target =>
                string.Equals(
                    target.Selector,
                    parseResult.Binding.Target.Selector,
                    StringComparison.OrdinalIgnoreCase));
            if (!_compatibilityAnalyzer.CanOverlap(
                    conditionResult.Condition,
                    targetDefinition.Applicability,
                    eventDefinition))
            {
                throw InvalidActionConfiguration();
            }

            var resolution = _eventFactory.ResolveTarget(
                campaignEvent,
                parseResult.Binding.Target.Selector);
            if (resolution.Status == CampaignTargetResolutionStatuses.NotApplicable)
            {
                continue;
            }

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

        if (executableActions.Count == 0)
        {
            return Skip(
                context,
                CampaignProcessingOutcomeCodes.TargetNotApplicable,
                operationTime);
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
        var envelope = _envelopeParser.Parse(
            Encoding.UTF8.GetBytes(context.NormalizedPayload),
            context.EventId.ToString("D"),
            context.EventType);
        var definition = await _definitionProvider.GetDefinitionAsync(
            envelope.EventType,
            envelope.EventVersion,
            cancellationToken)
            ?? throw InvalidConfiguration();
        var campaignEvent = _eventFactory.Create(
            envelope,
            definition,
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
            throw PersistenceFailure();
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
}
