using Core.Abstractions;
using Core.Entities.Constants;
using Core.UseCases.AuditLogs;
using Core.UseCases.Campaigns.Commands;
using Core.UseCases.Campaigns.Results;
using MediatR;
using DomainCampaign = Core.Entities.Campaign;
using DomainCampaignAction = Core.Entities.CampaignAction;

namespace Core.UseCases.Campaigns.Handlers;

public sealed class CreateCampaignCommandHandler
    : IRequestHandler<CreateCampaignCommand, CampaignDetailResult>
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly ICampaignEventDefinitionRepository _eventDefinitionRepository;
    private readonly IAuditLogWriter _auditLogWriter;
    private readonly ICampaignConfigurationService _configurationService;
    private readonly TimeProvider _timeProvider;

    public CreateCampaignCommandHandler(
        ICampaignRepository campaignRepository,
        ICampaignEventDefinitionRepository eventDefinitionRepository,
        IAuditLogWriter auditLogWriter,
        ICampaignConfigurationService configurationService,
        TimeProvider timeProvider)
    {
        _campaignRepository = campaignRepository;
        _eventDefinitionRepository = eventDefinitionRepository;
        _auditLogWriter = auditLogWriter;
        _configurationService = configurationService;
        _timeProvider = timeProvider;
    }

    public async Task<CampaignDetailResult> Handle(
        CreateCampaignCommand request,
        CancellationToken ct)
    {
        var eventDefinition = await GetSelectableEventDefinitionAsync(request.EventTypeVersionId, ct);
        var condition = _configurationService.ParseCondition(
            eventDefinition,
            request.ConditionJson);

        if (request.Actions is null || request.Actions.Count == 0)
        {
            throw new Core.Exceptions.DomainException(
                "CAMPAIGN_ACTIONS_REQUIRED",
                Core.Exceptions.DomainErrorType.Validation);
        }

        var parsedActions = request.Actions
            .Select(action => ParseAction(eventDefinition, action))
            .ToArray();
        EnsureUniqueActions(eventDefinition, parsedActions);

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var campaign = DomainCampaign.Create(
            request.CampaignName,
            request.Description,
            request.BannerImageUrl,
            eventDefinition.EventTypeVersionId,
            condition,
            request.StartDate.UtcDateTime,
            request.EndDate.UtcDateTime,
            request.ScheduleCron,
            request.DurationHour,
            request.UserLimitTotal,
            request.UserLimitSession,
            now);

        var actions = parsedActions
            .Select(action => DomainCampaignAction.Create(
                campaign.CampaignId,
                action.ActionType,
                action.ActionConfig,
                action.ExecuteOrder,
                action.TotalCount,
                action.SessionCount,
                now))
            .OrderBy(action => action.ExecuteOrder)
            .ThenBy(action => action.ActionId)
            .ToArray();

        _campaignRepository.Add(campaign);
        foreach (var action in actions)
        {
            _campaignRepository.AddAction(action);
        }

        _auditLogWriter.Add(new AuditLogEntry(
            request.ActorUserId,
            AuditActions.Create,
            AuditEntityTypes.Campaign,
            campaign.CampaignId,
            null,
            CampaignAuditSerializer.Campaign(campaign),
            null));
        foreach (var action in actions)
        {
            _auditLogWriter.Add(new AuditLogEntry(
                request.ActorUserId,
                AuditActions.Create,
                AuditEntityTypes.CampaignAction,
                action.ActionId,
                null,
                CampaignAuditSerializer.Action(action),
                null));
        }

        return campaign.ToDetailResult(eventDefinition) with
        {
            Actions = actions.Select(action => action.ToResult()).ToArray()
        };
    }

    private ParsedAction ParseAction(
        CampaignEventDefinitionResult eventDefinition,
        CreateCampaignActionInput input)
    {
        var (actionType, actionConfig) = _configurationService.ParseAction(
            eventDefinition,
            input.ActionType,
            input.ActionConfigJson);

        return new ParsedAction(
            actionType,
            actionConfig,
            input.ExecuteOrder,
            input.TotalCount,
            input.SessionCount);
    }

    private void EnsureUniqueActions(
        CampaignEventDefinitionResult eventDefinition,
        IReadOnlyCollection<ParsedAction> actions)
    {
        var executeOrders = new HashSet<int>();
        var actionKeys = new HashSet<string>(StringComparer.Ordinal);

        foreach (var action in actions)
        {
            if (!executeOrders.Add(action.ExecuteOrder))
            {
                throw new Core.Exceptions.DomainException(
                    "CAMPAIGN_ACTION_ORDER_CONFLICT",
                    Core.Exceptions.DomainErrorType.Conflict);
            }

            var key = _configurationService.GetActionUniquenessKey(
                eventDefinition,
                action.ActionType,
                action.ActionConfig);
            if (!actionKeys.Add(key))
            {
                throw new Core.Exceptions.DomainException(
                    "CAMPAIGN_ACTION_DUPLICATE",
                    Core.Exceptions.DomainErrorType.Conflict);
            }
        }
    }

    private async Task<CampaignEventDefinitionResult> GetSelectableEventDefinitionAsync(
        Guid eventTypeVersionId,
        CancellationToken ct)
    {
        if (eventTypeVersionId == Guid.Empty)
        {
            throw new Core.Exceptions.DomainException(
                "CAMPAIGN_EVENT_TYPE_VERSION_REQUIRED",
                Core.Exceptions.DomainErrorType.Validation);
        }

        return await _eventDefinitionRepository.GetForCampaignWriteAsync(eventTypeVersionId, ct)
            ?? throw new Core.Exceptions.DomainException(
                "CAMPAIGN_EVENT_TYPE_VERSION_NOT_FOUND",
                Core.Exceptions.DomainErrorType.Validation);
    }

    private sealed record ParsedAction(
        string ActionType,
        string ActionConfig,
        int ExecuteOrder,
        int? TotalCount,
        int? SessionCount);
}
