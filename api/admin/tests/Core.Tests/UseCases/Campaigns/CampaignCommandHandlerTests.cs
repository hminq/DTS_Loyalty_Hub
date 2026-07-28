using Campaign.Contracts.Constants;
using Core.Abstractions;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.UseCases.AuditLogs;
using Core.UseCases.Campaigns;
using Core.UseCases.Campaigns.Commands;
using Core.UseCases.Campaigns.Handlers;
using FluentAssertions;
using Messaging.Contracts.Events;
using Moq;
using DomainCampaign = Core.Entities.Campaign;
using DomainCampaignAction = Core.Entities.CampaignAction;

namespace Core.Tests.UseCases.Campaigns;

public sealed class CampaignCommandHandlerTests
{
    private static readonly DateTimeOffset FixedNow =
        new(2026, 7, 27, 10, 0, 0, TimeSpan.Zero);

    private readonly Mock<ICampaignRepository> _repository = new();
    private readonly Mock<IAuditLogWriter> _auditWriter = new();
    private readonly ICampaignConfigurationService _configurationService = new CampaignConfigurationService();
    private readonly TimeProvider _timeProvider = new FixedTimeProvider(FixedNow);

    public CampaignCommandHandlerTests()
    {
        _repository.Setup(repository => repository.GetActionsForUpdateAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
    }

    [Fact]
    public async Task CreateCampaign_ValidCommand_AddsDraftAndAudit()
    {
        _repository.Setup(repository => repository.Add(It.IsAny<DomainCampaign>()))
            .Returns((DomainCampaign campaign) => campaign);
        var command = ValidCreateCampaignCommand();
        var handler = new CreateCampaignCommandHandler(
            _repository.Object,
            _auditWriter.Object,
            _configurationService,
            _timeProvider);

        var result = await handler.Handle(command, CancellationToken.None);

        result.CampaignId.Should().NotBe(Guid.Empty);
        result.Status.Should().Be(CampaignStatuses.Draft);
        result.StartDate.Kind.Should().Be(DateTimeKind.Utc);
        result.Actions.Should().ContainSingle();
        result.Sessions.Should().BeEmpty();
        _repository.Verify(repository => repository.Add(
            It.Is<DomainCampaign>(campaign =>
                campaign.CampaignId == result.CampaignId &&
                campaign.EventType == EventTypeCodes.CustomerAccountRegistered &&
                campaign.Status == CampaignStatuses.Draft)), Times.Once);
        _repository.Verify(repository => repository.AddAction(
            It.Is<DomainCampaignAction>(action =>
                action.CampaignId == result.CampaignId &&
                action.ActionType == ActionTypes.IssuePoint &&
                action.ExecuteOrder == 1)), Times.Once);
        _auditWriter.Verify(writer => writer.Add(It.Is<AuditLogEntry>(entry =>
            entry.ActorUserId == command.ActorUserId &&
            entry.Action == AuditActions.Create &&
            entry.EntityType == AuditEntityTypes.Campaign &&
            entry.EntityId == result.CampaignId)), Times.Once);
        _auditWriter.Verify(writer => writer.Add(It.Is<AuditLogEntry>(entry =>
            entry.EntityType == AuditEntityTypes.CampaignAction &&
            entry.EntityId == result.Actions.Single().ActionId)), Times.Once);
    }

    [Fact]
    public async Task CreateCampaign_ReferralActionsWithDifferentRecipients_AreAccepted()
    {
        var command = ValidCreateCampaignCommand() with
        {
            ConditionJson = ReferralConditionJson,
            Actions =
            [
                ValidCreateActionInput("EVENT_CUSTOMER", 50, 1),
                ValidCreateActionInput("REFERRER", 100, 2)
            ]
        };
        var handler = new CreateCampaignCommandHandler(
            _repository.Object,
            _auditWriter.Object,
            _configurationService,
            _timeProvider);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Actions.Should().HaveCount(2);
        result.Actions.Select(action => action.ExecuteOrder).Should().ContainInOrder(1, 2);
        _repository.Verify(repository => repository.AddAction(
            It.IsAny<DomainCampaignAction>()), Times.Exactly(2));
    }

    [Fact]
    public async Task CreateCampaign_DuplicateActionConfig_RejectsBeforeMutation()
    {
        var command = ValidCreateCampaignCommand() with
        {
            Actions =
            [
                ValidCreateActionInput("EVENT_CUSTOMER", 50, 1),
                ValidCreateActionInput("EVENT_CUSTOMER", 50, 2)
            ]
        };
        var handler = new CreateCampaignCommandHandler(
            _repository.Object,
            _auditWriter.Object,
            _configurationService,
            _timeProvider);

        var action = () => handler.Handle(command, CancellationToken.None);

        var exception = await action.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorCode.Should().Be("CAMPAIGN_ACTION_DUPLICATE");
        _repository.Verify(repository => repository.Add(It.IsAny<DomainCampaign>()), Times.Never);
        _repository.Verify(repository => repository.AddAction(It.IsAny<DomainCampaignAction>()), Times.Never);
        _auditWriter.Verify(writer => writer.Add(It.IsAny<AuditLogEntry>()), Times.Never);
    }

    [Fact]
    public async Task CreateCampaign_InvalidScheduleCron_RejectsBeforeMutation()
    {
        var command = ValidCreateCampaignCommand() with
        {
            ScheduleCron = "0 42 15 * * ?."
        };
        var handler = new CreateCampaignCommandHandler(
            _repository.Object,
            _auditWriter.Object,
            _configurationService,
            _timeProvider);

        var action = () => handler.Handle(command, CancellationToken.None);

        var exception = await action.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorCode.Should().Be("CAMPAIGN_SCHEDULE_INVALID");
        _repository.Verify(
            repository => repository.Add(It.IsAny<DomainCampaign>()),
            Times.Never);
        _repository.Verify(
            repository => repository.AddAction(It.IsAny<DomainCampaignAction>()),
            Times.Never);
        _auditWriter.Verify(
            writer => writer.Add(It.IsAny<AuditLogEntry>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateCampaign_SameRecipientWithDifferentConfig_IsAccepted()
    {
        var command = ValidCreateCampaignCommand() with
        {
            Actions =
            [
                ValidCreateActionInput("EVENT_CUSTOMER", 50, 1),
                ValidCreateActionInput("EVENT_CUSTOMER", 100, 2)
            ]
        };
        var handler = new CreateCampaignCommandHandler(
            _repository.Object,
            _auditWriter.Object,
            _configurationService,
            _timeProvider);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Actions.Should().HaveCount(2);
        _repository.Verify(repository => repository.AddAction(
            It.IsAny<DomainCampaignAction>()), Times.Exactly(2));
    }

    [Fact]
    public async Task CreateCampaign_DuplicateOrder_RejectsBeforeMutation()
    {
        var command = ValidCreateCampaignCommand() with
        {
            ConditionJson = ReferralConditionJson,
            Actions =
            [
                ValidCreateActionInput("EVENT_CUSTOMER", 50, 1),
                ValidCreateActionInput("REFERRER", 100, 1)
            ]
        };
        var handler = new CreateCampaignCommandHandler(
            _repository.Object,
            _auditWriter.Object,
            _configurationService,
            _timeProvider);

        var action = () => handler.Handle(command, CancellationToken.None);

        var exception = await action.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorCode.Should().Be("CAMPAIGN_ACTION_ORDER_CONFLICT");
        _repository.Verify(repository => repository.Add(It.IsAny<DomainCampaign>()), Times.Never);
        _repository.Verify(repository => repository.AddAction(It.IsAny<DomainCampaignAction>()), Times.Never);
        _auditWriter.Verify(writer => writer.Add(It.IsAny<AuditLogEntry>()), Times.Never);
    }

    [Fact]
    public async Task CreateCampaign_InvalidSecondAction_RejectsBeforeMutation()
    {
        var command = ValidCreateCampaignCommand() with
        {
            Actions =
            [
                ValidCreateActionInput("EVENT_CUSTOMER", 50, 1),
                new CreateCampaignActionInput(
                    ActionTypes.IssuePoint,
                    "{}",
                    2,
                    null,
                    null)
            ]
        };
        var handler = new CreateCampaignCommandHandler(
            _repository.Object,
            _auditWriter.Object,
            _configurationService,
            _timeProvider);

        var action = () => handler.Handle(command, CancellationToken.None);

        var exception = await action.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorCode.Should().Be("CAMPAIGN_ACTION_CONFIG_INVALID");
        _repository.Verify(repository => repository.Add(It.IsAny<DomainCampaign>()), Times.Never);
        _repository.Verify(repository => repository.AddAction(It.IsAny<DomainCampaignAction>()), Times.Never);
        _auditWriter.Verify(writer => writer.Add(It.IsAny<AuditLogEntry>()), Times.Never);
    }

    [Fact]
    public async Task UpdateCampaign_ActiveCampaign_RejectsWithoutMutation()
    {
        var campaign = RestoredCampaign(CampaignStatuses.Active);
        _repository.Setup(repository => repository.GetForUpdateAsync(
                campaign.CampaignId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);
        _repository.Setup(repository => repository.GetByIdAsync(
                campaign.CampaignId,
                100,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CampaignDetail(campaign));
        var command = ValidUpdateCampaignCommand(campaign.CampaignId);
        var handler = new UpdateCampaignCommandHandler(
            _repository.Object,
            _auditWriter.Object,
            _configurationService,
            _timeProvider);

        var action = () => handler.Handle(command, CancellationToken.None);

        var exception = await action.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorCode.Should().Be("CAMPAIGN_NOT_DRAFT");
        _repository.Verify(repository => repository.UpdateAsync(
            It.IsAny<DomainCampaign>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _auditWriter.Verify(writer => writer.Add(It.IsAny<AuditLogEntry>()), Times.Never);
    }

    [Fact]
    public async Task UpdateCampaign_DraftCampaign_UpdatesAndAudits()
    {
        var campaign = RestoredCampaign(CampaignStatuses.Draft);
        _repository.Setup(repository => repository.GetForUpdateAsync(
                campaign.CampaignId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);
        _repository.Setup(repository => repository.GetByIdAsync(
                campaign.CampaignId,
                100,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CampaignDetail(campaign));
        var command = ValidUpdateCampaignCommand(campaign.CampaignId) with
        {
            CampaignName = "Updated registration reward"
        };
        var handler = new UpdateCampaignCommandHandler(
            _repository.Object,
            _auditWriter.Object,
            _configurationService,
            _timeProvider);

        var result = await handler.Handle(command, CancellationToken.None);

        result.CampaignName.Should().Be("Updated registration reward");
        result.UpdatedAt.Should().Be(FixedNow.UtcDateTime);
        _repository.Verify(repository => repository.UpdateAsync(
            It.Is<DomainCampaign>(item =>
                item.CampaignId == campaign.CampaignId &&
                item.CampaignName == "Updated registration reward"),
            It.IsAny<CancellationToken>()), Times.Once);
        _auditWriter.Verify(writer => writer.Add(It.Is<AuditLogEntry>(entry =>
            entry.Action == AuditActions.Update &&
            entry.EntityType == AuditEntityTypes.Campaign &&
            entry.EntityId == campaign.CampaignId &&
            entry.OldValue != null &&
            entry.NewValue != null)), Times.Once);
    }

    [Fact]
    public async Task CreateAction_ValidCommand_AddsActionTouchesCampaignAndAudits()
    {
        var campaign = RestoredCampaign(CampaignStatuses.Draft);
        _repository.Setup(repository => repository.GetForUpdateAsync(
                campaign.CampaignId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);
        _repository.Setup(repository => repository.ActionOrderExistsAsync(
                campaign.CampaignId,
                1,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repository.Setup(repository => repository.AddAction(It.IsAny<DomainCampaignAction>()))
            .Returns((DomainCampaignAction action) => action);
        var command = ValidCreateActionCommand(campaign.CampaignId);
        var handler = new CreateCampaignActionCommandHandler(
            _repository.Object,
            _auditWriter.Object,
            _configurationService,
            _timeProvider);

        var result = await handler.Handle(command, CancellationToken.None);

        result.ActionId.Should().NotBe(Guid.Empty);
        result.UsedCount.Should().Be(0);
        _repository.Verify(repository => repository.AddAction(
            It.Is<DomainCampaignAction>(action =>
                action.CampaignId == campaign.CampaignId &&
                action.ExecuteOrder == 1)), Times.Once);
        _repository.Verify(repository => repository.TouchAsync(
            campaign.CampaignId,
            FixedNow.UtcDateTime,
            It.IsAny<CancellationToken>()), Times.Once);
        _auditWriter.Verify(writer => writer.Add(It.Is<AuditLogEntry>(entry =>
            entry.Action == AuditActions.Create &&
            entry.EntityType == AuditEntityTypes.CampaignAction &&
            entry.EntityId == result.ActionId)), Times.Once);
    }

    [Fact]
    public async Task CreateAction_DuplicateOrder_RejectsWithoutMutation()
    {
        var campaign = RestoredCampaign(CampaignStatuses.Draft);
        _repository.Setup(repository => repository.GetForUpdateAsync(
                campaign.CampaignId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);
        _repository.Setup(repository => repository.ActionOrderExistsAsync(
                campaign.CampaignId,
                1,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var handler = new CreateCampaignActionCommandHandler(
            _repository.Object,
            _auditWriter.Object,
            _configurationService,
            _timeProvider);

        var action = () => handler.Handle(
            ValidCreateActionCommand(campaign.CampaignId),
            CancellationToken.None);

        var exception = await action.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorCode.Should().Be("CAMPAIGN_ACTION_ORDER_CONFLICT");
        _repository.Verify(repository => repository.AddAction(
            It.IsAny<DomainCampaignAction>()), Times.Never);
        _repository.Verify(repository => repository.TouchAsync(
            It.IsAny<Guid>(),
            It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _auditWriter.Verify(writer => writer.Add(It.IsAny<AuditLogEntry>()), Times.Never);
    }

    [Fact]
    public async Task CreateAction_DuplicateConfig_RejectsWithoutMutation()
    {
        var campaign = RestoredCampaign(CampaignStatuses.Draft);
        var existingAction = RestoredAction(campaign.CampaignId);
        _repository.Setup(repository => repository.GetForUpdateAsync(
                campaign.CampaignId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);
        _repository.Setup(repository => repository.ActionOrderExistsAsync(
                campaign.CampaignId,
                2,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repository.Setup(repository => repository.GetActionsForUpdateAsync(
                campaign.CampaignId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([existingAction]);
        var handler = new CreateCampaignActionCommandHandler(
            _repository.Object,
            _auditWriter.Object,
            _configurationService,
            _timeProvider);
        var command = ValidCreateActionCommand(campaign.CampaignId) with { ExecuteOrder = 2 };

        var action = () => handler.Handle(command, CancellationToken.None);

        var exception = await action.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorCode.Should().Be("CAMPAIGN_ACTION_DUPLICATE");
        _repository.Verify(repository => repository.AddAction(It.IsAny<DomainCampaignAction>()), Times.Never);
        _auditWriter.Verify(writer => writer.Add(It.IsAny<AuditLogEntry>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAction_ValidCommand_UpdatesActionAndAudits()
    {
        var campaign = RestoredCampaign(CampaignStatuses.Draft);
        var existingAction = RestoredAction(campaign.CampaignId);
        _repository.Setup(repository => repository.GetForUpdateAsync(
                campaign.CampaignId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);
        _repository.Setup(repository => repository.GetActionForUpdateAsync(
                campaign.CampaignId,
                existingAction.ActionId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAction);
        _repository.Setup(repository => repository.ActionOrderExistsAsync(
                campaign.CampaignId,
                2,
                existingAction.ActionId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repository.Setup(repository => repository.GetActionsForUpdateAsync(
                campaign.CampaignId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([existingAction]);
        var command = new UpdateCampaignActionCommand(
            campaign.CampaignId,
            existingAction.ActionId,
            ActionTypes.IssuePoint,
            """
            {"target":{"selector":"EVENT_CUSTOMER"},"parameters":{"amount":75}}
            """,
            2,
            10,
            5,
            Guid.NewGuid());
        var handler = new UpdateCampaignActionCommandHandler(
            _repository.Object,
            _auditWriter.Object,
            _configurationService,
            _timeProvider);

        var result = await handler.Handle(command, CancellationToken.None);

        result.ExecuteOrder.Should().Be(2);
        result.TotalCount.Should().Be(10);
        result.SessionCount.Should().Be(5);
        _repository.Verify(repository => repository.UpdateActionAsync(
            It.Is<DomainCampaignAction>(action =>
                action.ActionId == existingAction.ActionId &&
                action.ExecuteOrder == 2),
            FixedNow.UtcDateTime,
            It.IsAny<CancellationToken>()), Times.Once);
        _auditWriter.Verify(writer => writer.Add(It.Is<AuditLogEntry>(entry =>
            entry.Action == AuditActions.Update &&
            entry.EntityType == AuditEntityTypes.CampaignAction &&
            entry.EntityId == existingAction.ActionId)), Times.Once);
    }

    [Fact]
    public async Task UpdateAction_ExistingConfig_RejectsWithoutMutation()
    {
        var campaign = RestoredCampaign(
            CampaignStatuses.Draft,
            ReferralConditionJson);
        var eventCustomerAction = RestoredAction(campaign.CampaignId);
        var referrerAction = DomainCampaignAction.Restore(
            Guid.NewGuid(),
            campaign.CampaignId,
            ActionTypes.IssuePoint,
            """
            {"target":{"selector":"REFERRER"},"parameters":{"amount":100}}
            """,
            2,
            null,
            null,
            0,
            FixedNow.UtcDateTime);
        _repository.Setup(repository => repository.GetForUpdateAsync(
                campaign.CampaignId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);
        _repository.Setup(repository => repository.GetActionForUpdateAsync(
                campaign.CampaignId,
                eventCustomerAction.ActionId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventCustomerAction);
        _repository.Setup(repository => repository.ActionOrderExistsAsync(
                campaign.CampaignId,
                1,
                eventCustomerAction.ActionId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repository.Setup(repository => repository.GetActionsForUpdateAsync(
                campaign.CampaignId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([eventCustomerAction, referrerAction]);
        var handler = new UpdateCampaignActionCommandHandler(
            _repository.Object,
            _auditWriter.Object,
            _configurationService,
            _timeProvider);
        var command = new UpdateCampaignActionCommand(
            campaign.CampaignId,
            eventCustomerAction.ActionId,
            ActionTypes.IssuePoint,
            """
            {"target":{"selector":"REFERRER"},"parameters":{"amount":100}}
            """,
            1,
            null,
            null,
            Guid.NewGuid());

        var action = () => handler.Handle(command, CancellationToken.None);

        var exception = await action.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorCode.Should().Be("CAMPAIGN_ACTION_DUPLICATE");
        _repository.Verify(repository => repository.UpdateActionAsync(
            It.IsAny<DomainCampaignAction>(),
            It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _auditWriter.Verify(writer => writer.Add(It.IsAny<AuditLogEntry>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAction_DraftCampaign_DeletesAndAudits()
    {
        var campaign = RestoredCampaign(CampaignStatuses.Draft);
        var existingAction = RestoredAction(campaign.CampaignId);
        _repository.Setup(repository => repository.GetForUpdateAsync(
                campaign.CampaignId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);
        _repository.Setup(repository => repository.GetActionForUpdateAsync(
                campaign.CampaignId,
                existingAction.ActionId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAction);
        var command = new DeleteCampaignActionCommand(
            campaign.CampaignId,
            existingAction.ActionId,
            Guid.NewGuid());
        var handler = new DeleteCampaignActionCommandHandler(
            _repository.Object,
            _auditWriter.Object,
            _timeProvider);

        await handler.Handle(command, CancellationToken.None);

        _repository.Verify(repository => repository.DeleteActionAsync(
            campaign.CampaignId,
            existingAction.ActionId,
            FixedNow.UtcDateTime,
            It.IsAny<CancellationToken>()), Times.Once);
        _auditWriter.Verify(writer => writer.Add(It.Is<AuditLogEntry>(entry =>
            entry.Action == AuditActions.Delete &&
            entry.EntityType == AuditEntityTypes.CampaignAction &&
            entry.EntityId == existingAction.ActionId)), Times.Once);
    }

    [Fact]
    public async Task DeleteCampaign_Draft_DeletesActionsAndCampaignThroughRepositoryAndAudits()
    {
        var campaign = RestoredCampaign(CampaignStatuses.Draft);
        _repository.Setup(repository => repository.GetForUpdateAsync(
                campaign.CampaignId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);
        var command = new DeleteCampaignCommand(campaign.CampaignId, Guid.NewGuid());
        var handler = new DeleteCampaignCommandHandler(
            _repository.Object,
            _auditWriter.Object);

        await handler.Handle(command, CancellationToken.None);

        _repository.Verify(repository => repository.DeleteDraftAsync(
            campaign.CampaignId,
            It.IsAny<CancellationToken>()), Times.Once);
        _auditWriter.Verify(writer => writer.Add(It.Is<AuditLogEntry>(entry =>
            entry.ActorUserId == command.ActorUserId &&
            entry.Action == AuditActions.Delete &&
            entry.EntityType == AuditEntityTypes.Campaign &&
            entry.EntityId == campaign.CampaignId)), Times.Once);
    }

    [Theory]
    [InlineData(typeof(CreateCampaignCommand))]
    [InlineData(typeof(UpdateCampaignCommand))]
    [InlineData(typeof(DeleteCampaignCommand))]
    [InlineData(typeof(CreateCampaignActionCommand))]
    [InlineData(typeof(UpdateCampaignActionCommand))]
    [InlineData(typeof(DeleteCampaignActionCommand))]
    public void PhaseThreeWriteCommands_AreTransactional(Type commandType)
    {
        typeof(ITransactionalRequest).IsAssignableFrom(commandType).Should().BeTrue();
    }

    private static CreateCampaignCommand ValidCreateCampaignCommand() => new(
        "Normal registration reward",
        "Issue points after registration.",
        null,
        EventTypeCodes.CustomerAccountRegistered,
        NormalConditionJson,
        FixedNow.AddDays(1),
        FixedNow.AddDays(31),
        "0 0 2 * * ?",
        2,
        1,
        1,
        [ValidCreateActionInput("EVENT_CUSTOMER", 50, 1)],
        Guid.NewGuid());

    private static CreateCampaignActionInput ValidCreateActionInput(
        string selector,
        decimal amount,
        int executeOrder) => new(
        ActionTypes.IssuePoint,
        $$$"""
        {"target":{"selector":"{{{selector}}}"},"parameters":{"amount":{{{amount}}}}}
        """,
        executeOrder,
        null,
        null);

    private static UpdateCampaignCommand ValidUpdateCampaignCommand(Guid campaignId)
    {
        var create = ValidCreateCampaignCommand();
        return new UpdateCampaignCommand(
            campaignId,
            create.CampaignName,
            create.Description,
            create.BannerImageUrl,
            create.EventType,
            create.ConditionJson,
            create.StartDate,
            create.EndDate,
            create.ScheduleCron,
            create.DurationHour,
            create.UserLimitTotal,
            create.UserLimitSession,
            create.ActorUserId);
    }

    private static CreateCampaignActionCommand ValidCreateActionCommand(Guid campaignId) => new(
        campaignId,
        ActionTypes.IssuePoint,
        """
        {"target":{"selector":"EVENT_CUSTOMER"},"parameters":{"amount":50}}
        """,
        1,
        null,
        null,
        Guid.NewGuid());

    private static DomainCampaign RestoredCampaign(
        string status,
        string condition = NormalConditionJson,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string scheduleCron = "0 0 2 * * ?")
    {
        return DomainCampaign.Restore(
            Guid.NewGuid(),
            "Registration reward",
            null,
            null,
            EventTypeCodes.CustomerAccountRegistered,
            condition,
            startDate ?? FixedNow.UtcDateTime.AddDays(1),
            endDate ?? FixedNow.UtcDateTime.AddDays(31),
            scheduleCron,
            2,
            1,
            1,
            status,
            FixedNow.UtcDateTime,
            FixedNow.UtcDateTime);
    }

    private static DomainCampaignAction RestoredAction(Guid campaignId)
    {
        return DomainCampaignAction.Restore(
            Guid.NewGuid(),
            campaignId,
            ActionTypes.IssuePoint,
            """
            {"target":{"selector":"EVENT_CUSTOMER"},"parameters":{"amount":50}}
            """,
            1,
            null,
            null,
            0,
            FixedNow.UtcDateTime);
    }

    private const string NormalConditionJson =
        """{"all":[{"field":"source","operator":"EQUALS","value":"NORMAL"}]}""";

    private const string ReferralConditionJson =
        """{"all":[{"field":"source","operator":"EQUALS","value":"REFERRAL"}]}""";

    private static Core.UseCases.Campaigns.Results.CampaignDetailResult CampaignDetail(
        DomainCampaign campaign)
    {
        return new(
            campaign.CampaignId,
            campaign.CampaignName,
            campaign.Description,
            campaign.BannerImageUrl,
            null,
            campaign.EventType,
            campaign.StartDate,
            campaign.EndDate,
            campaign.Condition,
            campaign.ScheduleCron,
            campaign.DurationHour,
            campaign.UserLimitTotal,
            campaign.UserLimitSession,
            campaign.Status,
            campaign.CreatedAt,
            campaign.UpdatedAt,
            [],
            [],
            0);
    }

    [Fact]
    public async Task ActivateCampaign_ValidDraft_MaterializesSessionsAndSetsActive()
    {
        var campaign = RestoredCampaign(CampaignStatuses.Draft);
        var action = RestoredAction(campaign.CampaignId);
        _repository.Setup(repository => repository.GetForUpdateAsync(
                campaign.CampaignId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);
        _repository.Setup(repository => repository.GetActionsForUpdateAsync(
                campaign.CampaignId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([action]);

        var command = new ActivateCampaignCommand(campaign.CampaignId, Guid.NewGuid());
        var handler = new ActivateCampaignCommandHandler(
            _repository.Object,
            _auditWriter.Object,
            _configurationService,
            _timeProvider);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(CampaignStatuses.Active);
        result.SessionCount.Should().BeGreaterThan(0);
        result.Sessions.Should().NotBeEmpty();
        _repository.Verify(
            repository => repository.AddSessions(It.IsAny<IEnumerable<Core.Entities.CampaignSession>>()),
            Times.Once);
        _repository.Verify(
            repository => repository.UpdateAsync(campaign, It.IsAny<CancellationToken>()),
            Times.Once);
        _auditWriter.Verify(
            writer => writer.Add(It.Is<AuditLogEntry>(entry =>
                entry.Action == AuditActions.Activate &&
                entry.EntityType == AuditEntityTypes.Campaign &&
                entry.EntityId == campaign.CampaignId)),
            Times.Once);
    }

    [Fact]
    public async Task ActivateCampaign_StartedDraftWithFutureOccurrences_MaterializesOnlyFutureSessions()
    {
        var campaign = RestoredCampaign(
            CampaignStatuses.Draft,
            startDate: FixedNow.UtcDateTime.AddDays(-1),
            endDate: FixedNow.UtcDateTime.AddDays(2),
            scheduleCron: "0 0 12 * * ?");
        var action = RestoredAction(campaign.CampaignId);
        Core.Entities.CampaignSession[] addedSessions = [];
        _repository.Setup(repository => repository.GetForUpdateAsync(
                campaign.CampaignId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);
        _repository.Setup(repository => repository.GetActionsForUpdateAsync(
                campaign.CampaignId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([action]);
        _repository.Setup(repository => repository.AddSessions(
                It.IsAny<IEnumerable<Core.Entities.CampaignSession>>()))
            .Callback<IEnumerable<Core.Entities.CampaignSession>>(
                sessions => addedSessions = sessions.ToArray());

        var command = new ActivateCampaignCommand(campaign.CampaignId, Guid.NewGuid());
        var handler = new ActivateCampaignCommandHandler(
            _repository.Object,
            _auditWriter.Object,
            _configurationService,
            _timeProvider);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(CampaignStatuses.Active);
        addedSessions.Should().HaveCount(2);
        addedSessions.Should().OnlyContain(
            session => session.SessionStart >= FixedNow.UtcDateTime);
        addedSessions[0].SessionStart.Should().Be(
            new DateTime(2026, 7, 27, 12, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public async Task ActivateCampaign_EndedDraft_ThrowsScheduleEmpty()
    {
        var campaign = RestoredCampaign(
            CampaignStatuses.Draft,
            startDate: FixedNow.UtcDateTime.AddDays(-2),
            endDate: FixedNow.UtcDateTime.AddMinutes(-1));
        _repository.Setup(repository => repository.GetForUpdateAsync(
                campaign.CampaignId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);

        var command = new ActivateCampaignCommand(campaign.CampaignId, Guid.NewGuid());
        var handler = new ActivateCampaignCommandHandler(
            _repository.Object,
            _auditWriter.Object,
            _configurationService,
            _timeProvider);

        var act = () => handler.Handle(command, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<DomainException>();
        ex.Which.ErrorCode.Should().Be("CAMPAIGN_SCHEDULE_EMPTY");
        ex.Which.ErrorType.Should().Be(DomainErrorType.Validation);
        campaign.Status.Should().Be(CampaignStatuses.Draft);
        _repository.Verify(
            repository => repository.AddSessions(
                It.IsAny<IEnumerable<Core.Entities.CampaignSession>>()),
            Times.Never);
        _auditWriter.Verify(
            writer => writer.Add(It.IsAny<AuditLogEntry>()),
            Times.Never);
    }

    [Fact]
    public async Task ActivateCampaign_AlreadyActive_ThrowsConflict()
    {
        var campaign = RestoredCampaign(CampaignStatuses.Active);
        _repository.Setup(repository => repository.GetForUpdateAsync(
                campaign.CampaignId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);

        var command = new ActivateCampaignCommand(campaign.CampaignId, Guid.NewGuid());
        var handler = new ActivateCampaignCommandHandler(
            _repository.Object,
            _auditWriter.Object,
            _configurationService,
            _timeProvider);

        var act = () => handler.Handle(command, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<DomainException>();
        ex.Which.ErrorCode.Should().Be("CAMPAIGN_ALREADY_ACTIVE");
        ex.Which.ErrorType.Should().Be(DomainErrorType.Conflict);
    }

    [Fact]
    public async Task ActivateCampaign_NoActions_ThrowsValidation()
    {
        var campaign = RestoredCampaign(CampaignStatuses.Draft);
        _repository.Setup(repository => repository.GetForUpdateAsync(
                campaign.CampaignId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);
        _repository.Setup(repository => repository.GetActionsForUpdateAsync(
                campaign.CampaignId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var command = new ActivateCampaignCommand(campaign.CampaignId, Guid.NewGuid());
        var handler = new ActivateCampaignCommandHandler(
            _repository.Object,
            _auditWriter.Object,
            _configurationService,
            _timeProvider);

        var act = () => handler.Handle(command, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<DomainException>();
        ex.Which.ErrorCode.Should().Be("CAMPAIGN_ACTIONS_REQUIRED");
        ex.Which.ErrorType.Should().Be(DomainErrorType.Validation);
    }

    [Fact]
    public async Task CancelCampaign_ActiveCampaign_CancelsOpenSessionsAndAddsAudit()
    {
        var campaign = RestoredCampaign(CampaignStatuses.Active);
        var scheduledSession = RestoredSession(
            campaign.CampaignId,
            CampaignSessionStatuses.Scheduled,
            FixedNow.UtcDateTime.AddHours(2),
            FixedNow.UtcDateTime.AddHours(3));
        var runningSession = RestoredSession(
            campaign.CampaignId,
            CampaignSessionStatuses.Running,
            FixedNow.UtcDateTime.AddHours(-1),
            FixedNow.UtcDateTime.AddHours(1));
        Core.Entities.CampaignSession[] sessions =
        [
            scheduledSession,
            runningSession
        ];
        _repository.Setup(repository => repository.GetForUpdateAsync(
                campaign.CampaignId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);
        _repository.Setup(repository => repository.GetOpenSessionsForUpdateAsync(
                campaign.CampaignId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessions);

        var command = new CancelCampaignCommand(campaign.CampaignId, Guid.NewGuid());
        var handler = new CancelCampaignCommandHandler(
            _repository.Object,
            _auditWriter.Object,
            _timeProvider);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(CampaignStatuses.Cancelled);
        result.CancelledScheduledSessionCount.Should().Be(1);
        result.CancelledRunningSessionCount.Should().Be(1);
        result.CancelledAt.Should().Be(FixedNow.UtcDateTime);
        campaign.Status.Should().Be(CampaignStatuses.Cancelled);
        campaign.UpdatedAt.Should().Be(FixedNow.UtcDateTime);
        scheduledSession.Status.Should().Be(CampaignSessionStatuses.Cancelled);
        scheduledSession.EndedAt.Should().BeNull();
        runningSession.Status.Should().Be(CampaignSessionStatuses.Cancelled);
        runningSession.EndedAt.Should().Be(FixedNow.UtcDateTime);
        _repository.Verify(repository => repository.UpdateAsync(
            campaign,
            It.IsAny<CancellationToken>()), Times.Once);
        _repository.Verify(repository => repository.UpdateSessions(
            It.Is<IEnumerable<Core.Entities.CampaignSession>>(value =>
                value.SequenceEqual(sessions))), Times.Once);
        _auditWriter.Verify(writer => writer.Add(It.Is<AuditLogEntry>(entry =>
            entry.Action == AuditActions.Cancel &&
            entry.EntityType == AuditEntityTypes.Campaign &&
            entry.EntityId == campaign.CampaignId &&
            entry.OldValue != null &&
            entry.OldValue.Contains("\"status\":\"ACTIVE\"") &&
            entry.NewValue != null &&
            entry.NewValue.Contains("\"newStatus\":\"CANCELLED\"") &&
            entry.NewValue.Contains("\"cancelledScheduledSessionCount\":1") &&
            entry.NewValue.Contains("\"cancelledRunningSessionCount\":1"))),
            Times.Once);
    }

    [Theory]
    [InlineData(CampaignStatuses.Draft)]
    [InlineData(CampaignStatuses.Ended)]
    [InlineData(CampaignStatuses.Cancelled)]
    public async Task CancelCampaign_NonActiveCampaign_ThrowsConflictWithoutMutations(
        string status)
    {
        var campaign = RestoredCampaign(status);
        _repository.Setup(repository => repository.GetForUpdateAsync(
                campaign.CampaignId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);
        var handler = new CancelCampaignCommandHandler(
            _repository.Object,
            _auditWriter.Object,
            _timeProvider);

        var act = () => handler.Handle(
            new CancelCampaignCommand(campaign.CampaignId, Guid.NewGuid()),
            CancellationToken.None);

        var exception = await act.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorCode.Should().Be("CAMPAIGN_NOT_ACTIVE");
        exception.Which.ErrorType.Should().Be(DomainErrorType.Conflict);
        _repository.Verify(repository => repository.GetOpenSessionsForUpdateAsync(
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _repository.Verify(repository => repository.UpdateAsync(
            It.IsAny<DomainCampaign>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _repository.Verify(repository => repository.UpdateSessions(
            It.IsAny<IEnumerable<Core.Entities.CampaignSession>>()), Times.Never);
        _auditWriter.Verify(
            writer => writer.Add(It.IsAny<AuditLogEntry>()),
            Times.Never);
    }

    [Fact]
    public async Task CancelCampaign_MissingCampaign_ThrowsNotFound()
    {
        var campaignId = Guid.NewGuid();
        _repository.Setup(repository => repository.GetForUpdateAsync(
                campaignId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainCampaign?)null);
        var handler = new CancelCampaignCommandHandler(
            _repository.Object,
            _auditWriter.Object,
            _timeProvider);

        var act = () => handler.Handle(
            new CancelCampaignCommand(campaignId, Guid.NewGuid()),
            CancellationToken.None);

        var exception = await act.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorCode.Should().Be("CAMPAIGN_NOT_FOUND");
        exception.Which.ErrorType.Should().Be(DomainErrorType.NotFound);
        _auditWriter.Verify(
            writer => writer.Add(It.IsAny<AuditLogEntry>()),
            Times.Never);
    }

    private static Core.Entities.CampaignSession RestoredSession(
        Guid campaignId,
        string status,
        DateTime sessionStart,
        DateTime sessionEnd)
    {
        return Core.Entities.CampaignSession.Restore(
            Guid.NewGuid(),
            campaignId,
            sessionStart,
            sessionEnd,
            status,
            FixedNow.UtcDateTime.AddDays(-1),
            null);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
