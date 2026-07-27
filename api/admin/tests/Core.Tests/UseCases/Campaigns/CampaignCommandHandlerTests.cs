using Campaign.Contracts.Constants;
using Core.Abstractions;
using Core.Entities.Constants;
using Core.Exceptions;
using Core.UseCases.AuditLogs;
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
            ConditionJson = """{"sources":["REFERRAL"]}""",
            Actions =
            [
                ValidCreateActionInput("EVENT_CUSTOMER", 50, 1),
                ValidCreateActionInput("REFERRER", 100, 2)
            ]
        };
        var handler = new CreateCampaignCommandHandler(
            _repository.Object,
            _auditWriter.Object,
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
            _timeProvider);

        var action = () => handler.Handle(command, CancellationToken.None);

        var exception = await action.Should().ThrowAsync<DomainException>();
        exception.Which.ErrorCode.Should().Be("CAMPAIGN_ACTION_DUPLICATE");
        _repository.Verify(repository => repository.Add(It.IsAny<DomainCampaign>()), Times.Never);
        _repository.Verify(repository => repository.AddAction(It.IsAny<DomainCampaignAction>()), Times.Never);
        _auditWriter.Verify(writer => writer.Add(It.IsAny<AuditLogEntry>()), Times.Never);
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
            ConditionJson = """{"sources":["REFERRAL"]}""",
            Actions =
            [
                ValidCreateActionInput("EVENT_CUSTOMER", 50, 1),
                ValidCreateActionInput("REFERRER", 100, 1)
            ]
        };
        var handler = new CreateCampaignCommandHandler(
            _repository.Object,
            _auditWriter.Object,
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
                    null,
                    null,
                    null)
            ]
        };
        var handler = new CreateCampaignCommandHandler(
            _repository.Object,
            _auditWriter.Object,
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
            _timeProvider);

        var result = await handler.Handle(command, CancellationToken.None);

        result.ActionId.Should().NotBe(Guid.Empty);
        result.UsedCount.Should().Be(0);
        result.UsedAmount.Should().Be(0);
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
            {"calculationType":"FIXED_AMOUNT","recipient":"EVENT_CUSTOMER","amount":75,
             "calculationBase":null,"percentage":null,"maximumPoints":null}
            """,
            2,
            10,
            5,
            750,
            375,
            Guid.NewGuid());
        var handler = new UpdateCampaignActionCommandHandler(
            _repository.Object,
            _auditWriter.Object,
            _timeProvider);

        var result = await handler.Handle(command, CancellationToken.None);

        result.ExecuteOrder.Should().Be(2);
        result.TotalCount.Should().Be(10);
        result.SessionCount.Should().Be(5);
        _repository.Verify(repository => repository.UpdateActionAsync(
            It.Is<DomainCampaignAction>(action =>
                action.ActionId == existingAction.ActionId &&
                action.ExecuteOrder == 2 &&
                action.TotalAmount == 750),
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
            """{"sources":["REFERRAL"]}""");
        var eventCustomerAction = RestoredAction(campaign.CampaignId);
        var referrerAction = DomainCampaignAction.Restore(
            Guid.NewGuid(),
            campaign.CampaignId,
            ActionTypes.IssuePoint,
            """
            {"calculationType":"FIXED_AMOUNT","recipient":"REFERRER","amount":100,
             "calculationBase":null,"percentage":null,"maximumPoints":null}
            """,
            2,
            null,
            null,
            null,
            null,
            0,
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
            _timeProvider);
        var command = new UpdateCampaignActionCommand(
            campaign.CampaignId,
            eventCustomerAction.ActionId,
            ActionTypes.IssuePoint,
            """
            {"calculationType":"FIXED_AMOUNT","recipient":"REFERRER","amount":100,
             "calculationBase":null,"percentage":null,"maximumPoints":null}
            """,
            1,
            null,
            null,
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
        """{"sources":["NORMAL"]}""",
        FixedNow.AddDays(1),
        FixedNow.AddDays(31),
        "0 0 2 * * ?",
        2,
        1,
        1,
        [ValidCreateActionInput("EVENT_CUSTOMER", 50, 1)],
        Guid.NewGuid());

    private static CreateCampaignActionInput ValidCreateActionInput(
        string recipient,
        decimal amount,
        int executeOrder) => new(
        ActionTypes.IssuePoint,
        $$"""
        {"calculationType":"FIXED_AMOUNT","recipient":"{{recipient}}","amount":{{amount}},
         "calculationBase":null,"percentage":null,"maximumPoints":null}
        """,
        executeOrder,
        null,
        null,
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
        {"calculationType":"FIXED_AMOUNT","recipient":"EVENT_CUSTOMER","amount":50,
         "calculationBase":null,"percentage":null,"maximumPoints":null}
        """,
        1,
        null,
        null,
        null,
        null,
        Guid.NewGuid());

    private static DomainCampaign RestoredCampaign(
        string status,
        string condition = """{"sources":["NORMAL"]}""")
    {
        return DomainCampaign.Restore(
            Guid.NewGuid(),
            "Registration reward",
            null,
            null,
            EventTypeCodes.CustomerAccountRegistered,
            condition,
            FixedNow.UtcDateTime.AddDays(1),
            FixedNow.UtcDateTime.AddDays(31),
            "0 0 2 * * ?",
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
            {"calculationType":"FIXED_AMOUNT","recipient":"EVENT_CUSTOMER","amount":50,
             "calculationBase":null,"percentage":null,"maximumPoints":null}
            """,
            1,
            null,
            null,
            null,
            null,
            0,
            0,
            FixedNow.UtcDateTime);
    }

    private static Core.UseCases.Campaigns.Results.CampaignDetailResult CampaignDetail(
        DomainCampaign campaign)
    {
        return new(
            campaign.CampaignId,
            campaign.CampaignName,
            campaign.Description,
            campaign.BannerImageUrl,
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

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
